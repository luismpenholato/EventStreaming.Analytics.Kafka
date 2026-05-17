using Analytics.Application.Processing;
using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Handlers;

public sealed class UserLoggedInHandler(IMetricsRepository metrics, ILogger<UserLoggedInHandler> logger)
    : BaseEventHandler<UserLoggedInEvent>(metrics, logger)
{
    protected override Task ProcessAsync(UserLoggedInEvent @event, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
