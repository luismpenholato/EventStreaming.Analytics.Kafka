using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Processing;

public abstract class BaseEventHandler<TEvent>(IMetricsRepository metrics, ILogger logger) : IEventHandler<TEvent>
    where TEvent : IntegrationEvent
{
    protected IMetricsRepository Metrics { get; } = metrics;

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["EventId"] = @event.EventId,
            ["EventType"] = @event.EventType,
            ["CorrelationId"] = @event.CorrelationId
        }))
        {
            var processed = await Metrics.ProcessIdempotentlyAsync(
                @event.EventId,
                @event.EventType,
                @event.OccurredAt,
                ct => ProcessAsync(@event, ct),
                cancellationToken);

            if (!processed)
                logger.LogInformation("Skipping duplicate event {EventId}", @event.EventId);
        }
    }

    protected abstract Task ProcessAsync(TEvent @event, CancellationToken cancellationToken);
}
