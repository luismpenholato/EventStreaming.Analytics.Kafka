using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Testcontainers.PostgreSql;
using Tracking.Application.Abstractions;

namespace Tracking.IntegrationTests;

public sealed class EventsEndpointTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("analytics")
        .WithUsername("analytics")
        .WithPassword("analytics")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:TrackingDb"] = _postgres.GetConnectionString(),
                    ["Kafka:BootstrapServers"] = "localhost:9092"
                });
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IOutboxWriter));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddSingleton(Substitute.For<IOutboxWriter>());
            });
        });

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Product_viewed_should_return_bad_request_when_invalid()
    {
        var response = await _client.PostAsJsonAsync("/api/events/product-viewed", new { userId = "", productId = "" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Product_viewed_should_return_accepted_when_valid()
    {
        var response = await _client.PostAsJsonAsync("/api/events/product-viewed", new
        {
            userId = "user-1",
            productId = "prod-1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }
}
