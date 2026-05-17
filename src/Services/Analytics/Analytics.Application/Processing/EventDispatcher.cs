using BuildingBlocks.Contracts.Serialization;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Processing;

public sealed class EventDispatcher(IEventHandlerInvoker invoker, ILogger<EventDispatcher> logger)
{
    public async Task DispatchAsync(string eventType, string payload, CancellationToken cancellationToken)
    {
        if (!IntegrationEventRegistry.IsRegistered(eventType))
            throw new InvalidOperationException($"Unknown event type: {eventType}");

        var @event = IntegrationEventRegistry.Deserialize(eventType, payload);
        logger.LogDebug(
            "Dispatching {EventType} {EventId} {CorrelationId}",
            @event.EventType, @event.EventId, @event.CorrelationId);

        await invoker.InvokeAsync(@event, cancellationToken);
    }
}
