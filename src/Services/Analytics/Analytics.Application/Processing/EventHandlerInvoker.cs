using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application.Processing;

public interface IEventHandlerInvoker
{
    Task InvokeAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);
}

public sealed class EventHandlerInvoker(IServiceProvider serviceProvider) : IEventHandlerInvoker
{
    public Task InvokeAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var method = typeof(EventHandlerInvoker)
            .GetMethod(nameof(InvokeInternal), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .MakeGenericMethod(@event.GetType());

        return (Task)method.Invoke(this, [@event, cancellationToken])!;
    }

    private Task InvokeInternal<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : IntegrationEvent =>
        serviceProvider.GetRequiredService<IEventHandler<TEvent>>().HandleAsync(@event, cancellationToken);
}
