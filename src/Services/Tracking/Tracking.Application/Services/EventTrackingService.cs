using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Domain.Results;
using Microsoft.Extensions.Logging;
using Tracking.Application.Abstractions;

namespace Tracking.Application.Services;

public sealed class EventTrackingService(IOutboxWriter outbox, ILogger<EventTrackingService> logger)
    : IEventTrackingService
{
    public async Task<Result<Guid>> TrackAsync<TEvent>(
        TEvent @event,
        string topic,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        try
        {
            await outbox.EnqueueAsync(topic, @event, cancellationToken);
            logger.LogInformation(
                "Enqueued {EventType} {EventId} {CorrelationId} to outbox for topic {Topic}",
                @event.EventType, @event.EventId, @event.CorrelationId, topic);
            return Result<Guid>.Success(@event.EventId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue {EventType} {EventId}", @event.EventType, @event.EventId);
            return Result<Guid>.Failure("Failed to persist event to outbox.");
        }
    }
}
