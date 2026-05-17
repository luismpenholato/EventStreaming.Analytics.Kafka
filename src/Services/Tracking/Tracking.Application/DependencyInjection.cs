using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Tracking.Application.Abstractions;
using Tracking.Application.Services;

namespace Tracking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTrackingApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<IRequestValidator, FluentRequestValidator>();
        services.AddScoped<IEventTrackingService, EventTrackingService>();
        return services;
    }
}
