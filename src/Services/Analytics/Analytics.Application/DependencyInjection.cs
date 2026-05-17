using Analytics.Application.Handlers;
using Analytics.Application.Processing;
using BuildingBlocks.Contracts.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsApplication(this IServiceCollection services)
    {
        services.AddScoped<IEventHandlerInvoker, EventHandlerInvoker>();
        services.AddScoped<EventDispatcher>();
        services.AddScoped<IEventHandler<ProductViewedEvent>, ProductViewedHandler>();
        services.AddScoped<IEventHandler<ProductAddedToCartEvent>, ProductAddedToCartHandler>();
        services.AddScoped<IEventHandler<CheckoutStartedEvent>, CheckoutStartedHandler>();
        services.AddScoped<IEventHandler<OrderCompletedEvent>, OrderCompletedHandler>();
        services.AddScoped<IEventHandler<UserLoggedInEvent>, UserLoggedInHandler>();
        return services;
    }
}
