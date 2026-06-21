using Analytics.Application.Processing;
using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers;

public sealed class OrderCompletedHandler(IMetricsRepository metrics, ILogger<OrderCompletedHandler> logger)
    : BaseEventHandler<OrderCompletedEvent>(metrics, logger)
{
    protected override Task ProcessAsync(OrderCompletedEvent @event, CancellationToken cancellationToken) =>
        Metrics.IncrementOrderCompletedAsync(@event.ProductIds, cancellationToken);
}
