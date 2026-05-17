using Analytics.Application.Handlers;
using Analytics.Application.Processing;
using Analytics.Domain.Abstractions;
using BuildingBlocks.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Analytics.UnitTests.Processing;

public sealed class EventDispatcherTests
{
    [Fact]
    public async Task Should_dispatch_product_viewed_using_registry()
    {
        var metrics = Substitute.For<IMetricsRepository>();
        metrics.ProcessIdempotentlyAsync(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<DateTimeOffset>(),
                Arg.Any<Func<CancellationToken, Task>>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<ILogger<EventDispatcher>>());
        services.AddSingleton(Substitute.For<ILogger<ProductViewedHandler>>());
        services.AddScoped<IEventHandlerInvoker, EventHandlerInvoker>();
        services.AddScoped<EventDispatcher>();
        services.AddScoped<IEventHandler<ProductViewedEvent>, ProductViewedHandler>();
        services.AddSingleton(metrics);

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<EventDispatcher>();

        var @event = new ProductViewedEvent
        {
            EventId = Guid.NewGuid(),
            UserId = "u1",
            ProductId = "p1"
        };

        var payload = BuildingBlocks.Contracts.Serialization.IntegrationEventRegistry.Serialize(@event);
        await dispatcher.DispatchAsync(ProductViewedEvent.TypeName, payload, CancellationToken.None);

        await metrics.DidNotReceive().IncrementProductViewAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
