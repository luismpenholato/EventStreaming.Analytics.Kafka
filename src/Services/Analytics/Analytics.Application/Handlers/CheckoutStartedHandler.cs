using Analytics.Application.Processing;
using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers;

/// <summary>
/// Event-type and period metrics are updated in ProcessIdempotentlyAsync.
/// </summary>
public sealed class CheckoutStartedHandler(IMetricsRepository metrics, ILogger<CheckoutStartedHandler> logger)
    : BaseEventHandler<CheckoutStartedEvent>(metrics, logger)
{
    protected override Task ProcessAsync(CheckoutStartedEvent @event, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
