using Analytics.Application.Processing;
using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers;

public sealed class ProductViewedHandler(IMetricsRepository metrics, ILogger<ProductViewedHandler> logger)
    : BaseEventHandler<ProductViewedEvent>(metrics, logger)
{
    protected override Task ProcessAsync(ProductViewedEvent @event, CancellationToken cancellationToken) =>
        Metrics.IncrementProductViewAsync(@event.ProductId, cancellationToken);
}
