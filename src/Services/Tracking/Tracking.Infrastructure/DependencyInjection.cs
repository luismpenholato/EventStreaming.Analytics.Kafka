using BuildingBlocks.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tracking.Application.Abstractions;
using Tracking.Infrastructure.Outbox;
using Tracking.Infrastructure.Persistence;

namespace Tracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTrackingPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TrackingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("TrackingDb")));

        return services;
    }

    public static IServiceCollection AddTrackingOutbox(this IServiceCollection services)
    {
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<IOutboxPublisher, OutboxPublisher>();
        return services;
    }

    public static IServiceCollection AddTrackingMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKafkaProducer(configuration);
        services.AddHostedService<OutboxPublisherWorker>();
        return services;
    }

    public static IServiceCollection AddTrackingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTrackingPersistence(configuration);
        services.AddTrackingOutbox();
        services.AddTrackingMessaging(configuration);
        return services;
    }

    public static async Task MigrateTrackingDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
