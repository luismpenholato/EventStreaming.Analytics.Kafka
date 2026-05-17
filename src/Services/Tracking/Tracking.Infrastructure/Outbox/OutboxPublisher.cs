using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Serialization;
using BuildingBlocks.Kafka.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Tracking.Application.Abstractions;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure.Outbox;

public sealed class OutboxPublisher(
    TrackingDbContext db,
    IIntegrationEventPublisher publisher,
    IOptions<KafkaOptions> kafkaOptions,
    ILogger<OutboxPublisher> logger) : IOutboxPublisher
{
    private const int BatchSize = 20;

    public async Task<int> PublishPendingBatchAsync(CancellationToken cancellationToken = default)
    {
        var maxRetries = kafkaOptions.Value.MaxRetryAttempts;

        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < maxRetries)
            .OrderBy(m => m.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            using (LogContext.PushProperty("CorrelationId", message.CorrelationId))
            using (LogContext.PushProperty("EventId", message.EventId))
            using (LogContext.PushProperty("EventType", message.EventType))
            using (LogContext.PushProperty("Topic", message.Topic))
            {
                try
                {
                    var @event = IntegrationEventRegistry.Deserialize(message.EventType, message.Payload);
                    await PublishAsync(publisher, message.Topic, @event, cancellationToken);

                    message.ProcessedAt = DateTimeOffset.UtcNow;
                    message.LastError = null;
                    logger.LogInformation("Outbox message {OutboxId} published to {Topic}", message.Id, message.Topic);
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.LastError = ex.Message;
                    logger.LogWarning(ex,
                        "Outbox publish failed for {OutboxId}. Retry {RetryCount}/{MaxRetries}",
                        message.Id, message.RetryCount, maxRetries);
                }
            }
        }

        if (pending.Count > 0)
            await db.SaveChangesAsync(cancellationToken);

        return pending.Count(m => m.ProcessedAt is not null);
    }

    private static Task PublishAsync(
        IIntegrationEventPublisher publisher,
        string topic,
        IntegrationEvent @event,
        CancellationToken cancellationToken)
    {
        var method = typeof(IIntegrationEventPublisher)
            .GetMethod(nameof(IIntegrationEventPublisher.PublishAsync))!
            .MakeGenericMethod(@event.GetType());

        return (Task)method.Invoke(publisher, [topic, @event, cancellationToken])!;
    }
}
