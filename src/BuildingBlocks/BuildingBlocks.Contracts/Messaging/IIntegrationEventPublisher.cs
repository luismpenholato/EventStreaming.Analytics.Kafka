using BuildingBlocks.Contracts.Events;

namespace BuildingBlocks.Contracts.Messaging;

public interface IIntegrationEventPublisher
{
    Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
