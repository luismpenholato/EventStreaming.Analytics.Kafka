using Analytics.Application.Processing;
using BuildingBlocks.Contracts.Serialization;
using BuildingBlocks.Contracts.Topics;
using Serilog.Context;
using BuildingBlocks.Kafka.Configuration;
using BuildingBlocks.Kafka.Consumers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Analytics.Consumer.Workers;

public sealed class AnalyticsKafkaConsumer : KafkaBackgroundConsumer
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AnalyticsKafkaConsumer(
        IOptions<KafkaOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<AnalyticsKafkaConsumer> logger)
        : base(options, logger,
            KafkaTopics.UserActivityEvents,
            KafkaTopics.OrderEvents)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ProcessMessageAsync(
        string topic,
        string eventType,
        string payload,
        CancellationToken cancellationToken)
    {
        var @event = IntegrationEventRegistry.Deserialize(eventType, payload);
        using (LogContext.PushProperty("CorrelationId", @event.CorrelationId))
        using (LogContext.PushProperty("EventId", @event.EventId))
        using (LogContext.PushProperty("EventType", @event.EventType))
        using (LogContext.PushProperty("Topic", topic))
        {
            using var scope = _scopeFactory.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<EventDispatcher>();
            await dispatcher.DispatchAsync(eventType, payload, cancellationToken);
        }
    }
}
