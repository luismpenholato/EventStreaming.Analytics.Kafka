using Analytics.Application.Handlers;
using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Analytics.UnitTests.Handlers;

public sealed class ProductViewedHandlerTests
{
    [Fact]
    public async Task Should_skip_when_event_already_processed()
    {
        var metrics = Substitute.For<IMetricsRepository>();
        metrics.ProcessIdempotentlyAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<Func<CancellationToken, Task>>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var handler = new ProductViewedHandler(metrics, Substitute.For<ILogger<ProductViewedHandler>>());
        await handler.HandleAsync(new ProductViewedEvent
        {
            EventId = Guid.NewGuid(),
            UserId = "u1",
            ProductId = "p1"
        });

        await metrics.DidNotReceive().IncrementProductViewAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_increment_metrics_for_new_event()
    {
        var metrics = Substitute.For<IMetricsRepository>();
        metrics.ProcessIdempotentlyAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<Func<CancellationToken, Task>>(),
                Arg.Any<CancellationToken>())
            .Returns(async callInfo =>
            {
                var process = callInfo.Arg<Func<CancellationToken, Task>>();
                await process(callInfo.Arg<CancellationToken>());
                return true;
            });

        var handler = new ProductViewedHandler(metrics, Substitute.For<ILogger<ProductViewedHandler>>());
        await handler.HandleAsync(new ProductViewedEvent
        {
            EventId = Guid.NewGuid(),
            UserId = "u1",
            ProductId = "p1"
        });

        await metrics.Received(1).IncrementProductViewAsync("p1", Arg.Any<CancellationToken>());
    }
}
