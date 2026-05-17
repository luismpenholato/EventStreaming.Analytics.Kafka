using Analytics.QueryApplication.Abstractions;
using Analytics.QueryInfrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Analytics.QueryInfrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddQueryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AnalyticsDb")
            ?? throw new InvalidOperationException("Connection string 'AnalyticsDb' is required.");

        services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        services.AddScoped<IAnalyticsQueries, DapperAnalyticsQueries>();
        return services;
    }
}
