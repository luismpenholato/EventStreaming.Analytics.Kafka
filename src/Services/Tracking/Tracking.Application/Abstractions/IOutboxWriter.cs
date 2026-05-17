using BuildingBlocks.Contracts.Events;

namespace Tracking.Application.Abstractions;

public interface IOutboxWriter
{
    Task EnqueueAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
