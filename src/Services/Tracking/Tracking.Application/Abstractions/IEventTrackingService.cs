using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Domain.Results;

namespace Tracking.Application.Abstractions;

public interface IEventTrackingService
{
    Task<Result<Guid>> TrackAsync<TEvent>(TEvent @event, string topic, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}
