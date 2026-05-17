using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Serialization;
using Tracking.Application.Abstractions;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure.Outbox;

public sealed class OutboxWriter(TrackingDbContext db) : IOutboxWriter
{
    public async Task EnqueueAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventId = @event.EventId,
            EventType = @event.EventType,
            Topic = topic,
            Payload = IntegrationEventRegistry.Serialize(@event),
            CorrelationId = @event.CorrelationId,
            CreatedAt = DateTimeOffset.UtcNow,
            RetryCount = 0
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
