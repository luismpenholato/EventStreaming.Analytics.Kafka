using Analytics.Domain.Abstractions;
using Analytics.Infrastructure;
using Analytics.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Analytics.IntegrationTests;

public sealed class MetricsRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("analytics")
        .WithUsername("analytics")
        .WithPassword("analytics")
        .Build();

    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AnalyticsDb"] = _postgres.GetConnectionString()
            })
            .Build();

        var services = new ServiceCollection();
        services.AddAnalyticsInfrastructure(config);
        _provider = services.BuildServiceProvider();
        await _provider.MigrateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_persist_product_view_metric_idempotently()
    {
        var repository = _provider.GetRequiredService<IMetricsRepository>();
        var eventId = Guid.NewGuid();

        var first = await repository.ProcessIdempotentlyAsync(
            eventId,
            "ProductViewed",
            DateTimeOffset.UtcNow,
            ct => repository.IncrementProductViewAsync("prod-99", ct));

        var second = await repository.ProcessIdempotentlyAsync(
            eventId,
            "ProductViewed",
            DateTimeOffset.UtcNow,
            ct => repository.IncrementProductViewAsync("prod-99", ct));

        first.Should().BeTrue();
        second.Should().BeFalse();

        var db = _provider.GetRequiredService<AnalyticsDbContext>();
        var metric = await db.ProductMetrics.FirstAsync(m => m.ProductId == "prod-99");
        metric.ViewCount.Should().Be(1);
    }
}
