using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Tracking.Application.Abstractions;

namespace Tracking.IntegrationTests;

public sealed class EventsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public EventsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IOutboxWriter));
                if (descriptor is not null)
                    services.Remove(descriptor);

                var outbox = Substitute.For<IOutboxWriter>();
                services.AddSingleton(outbox);
            });
        }).CreateClient();
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
