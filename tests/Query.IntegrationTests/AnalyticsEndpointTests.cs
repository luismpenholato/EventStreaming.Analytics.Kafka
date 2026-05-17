using Analytics.Domain.Abstractions;
using Analytics.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Query.IntegrationTests;

public sealed class AnalyticsEndpointTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("analytics")
        .WithUsername("analytics")
        .WithPassword("analytics")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:AnalyticsDb"] = _postgres.GetConnectionString()
                });
            });
        });

        using var scope = _factory.Services.CreateScope();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var services = new ServiceCollection();
        services.AddAnalyticsInfrastructure(config);
        var sp = services.BuildServiceProvider();
        await sp.MigrateDatabaseAsync();

        var metrics = sp.GetRequiredService<IMetricsRepository>();
        await metrics.ProcessIdempotentlyAsync(
            Guid.NewGuid(),
            "ProductViewed",
            DateTimeOffset.UtcNow,
            ct => metrics.IncrementProductViewAsync("prod-query", ct));
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_return_product_metrics()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/analytics/products/prod-query");
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Should_return_summary()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/analytics/summary");
        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
