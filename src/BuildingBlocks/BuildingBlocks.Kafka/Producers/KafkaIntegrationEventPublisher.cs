using System.Text;
using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Messaging;
using BuildingBlocks.Contracts.Serialization;
using BuildingBlocks.Kafka.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Kafka.Producers;

public sealed class KafkaIntegrationEventPublisher : IIntegrationEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaIntegrationEventPublisher> _logger;

    public KafkaIntegrationEventPublisher(IOptions<KafkaOptions> options, ILogger<KafkaIntegrationEventPublisher> logger)
    {
        _logger = logger;
        _producer = new ProducerBuilder<string, string>(new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            ClientId = options.Value.ClientId,
            Acks = Acks.All,
            EnableIdempotence = true
        }).Build();
    }

    public async Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var message = new Message<string, string>
        {
            Key = @event.EventId.ToString(),
            Value = IntegrationEventRegistry.Serialize(@event),
            Headers = new Headers
            {
                { "event-type", Encoding.UTF8.GetBytes(@event.EventType) },
                { "correlation-id", Encoding.UTF8.GetBytes(@event.CorrelationId.ToString()) }
            }
        };

        var result = await _producer.ProduceAsync(topic, message, cancellationToken);
        _logger.LogInformation(
            "Kafka published {EventType} {EventId} {CorrelationId} to {Topic} partition {Partition} offset {Offset}",
            @event.EventType, @event.EventId, @event.CorrelationId,
            topic, result.Partition.Value, result.Offset.Value);
    }

    public void Dispose() => _producer.Dispose();
}
