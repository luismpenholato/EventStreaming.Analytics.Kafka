using Analytics.Application.Processing;
using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers;

public sealed class ProductAddedToCartHandler(IMetricsRepository metrics, ILogger<ProductAddedToCartHandler> logger)
    : BaseEventHandler<ProductAddedToCartEvent>(metrics, logger)
{
    protected override Task ProcessAsync(ProductAddedToCartEvent @event, CancellationToken cancellationToken) =>
        metrics.IncrementAddedToCartAsync(@event.ProductId, cancellationToken);
}
