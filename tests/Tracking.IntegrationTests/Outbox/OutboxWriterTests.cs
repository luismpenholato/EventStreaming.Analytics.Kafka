using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Topics;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Tracking.Application.Abstractions;
using Tracking.Infrastructure;
using Tracking.Infrastructure.Persistence;

namespace Tracking.IntegrationTests.Outbox;

public sealed class OutboxWriterTests : IAsyncLifetime
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
                ["ConnectionStrings:TrackingDb"] = _postgres.GetConnectionString()
            })
            .Build();

        var services = new ServiceCollection();
        services.AddTrackingPersistence(config);
        services.AddTrackingOutbox();
        _provider = services.BuildServiceProvider();
        await _provider.MigrateTrackingDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_persist_event_in_outbox_without_publishing_immediately()
    {
        var writer = _provider.GetRequiredService<IOutboxWriter>();
        var eventId = Guid.NewGuid();
        var @event = new ProductViewedEvent
        {
            EventId = eventId,
            UserId = "user-1",
            ProductId = "prod-1",
            CorrelationId = Guid.NewGuid()
        };

        await writer.EnqueueAsync(KafkaTopics.UserActivityEvents, @event);

        var db = _provider.GetRequiredService<TrackingDbContext>();
        var message = await db.OutboxMessages.SingleAsync(m => m.EventId == eventId);
        message.ProcessedAt.Should().BeNull();
        message.Topic.Should().Be(KafkaTopics.UserActivityEvents);
        message.RetryCount.Should().Be(0);
    }
}
