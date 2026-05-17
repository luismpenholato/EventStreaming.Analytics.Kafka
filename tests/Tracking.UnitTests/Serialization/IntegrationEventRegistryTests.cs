using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Serialization;
using FluentAssertions;

namespace Tracking.UnitTests.Serialization;

public sealed class IntegrationEventRegistryTests
{
    [Theory]
    [InlineData(ProductViewedEvent.TypeName)]
    [InlineData(ProductAddedToCartEvent.TypeName)]
    [InlineData(CheckoutStartedEvent.TypeName)]
    [InlineData(OrderCompletedEvent.TypeName)]
    [InlineData(UserLoggedInEvent.TypeName)]
    public void Should_register_all_event_types(string eventType) =>
        IntegrationEventRegistry.IsRegistered(eventType).Should().BeTrue();

    [Fact]
    public void Should_roundtrip_product_viewed_event()
    {
        var original = new ProductViewedEvent
        {
            EventId = Guid.NewGuid(),
            UserId = "user-1",
            ProductId = "prod-1",
            CorrelationId = Guid.NewGuid()
        };

        var json = IntegrationEventRegistry.Serialize(original);
        var deserialized = IntegrationEventRegistry.Deserialize(ProductViewedEvent.TypeName, json);

        deserialized.Should().BeOfType<ProductViewedEvent>();
        deserialized.EventId.Should().Be(original.EventId);
    }
}
