using BuildingBlocks.Contracts.Events;

namespace Analytics.Application.Processing;

public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
