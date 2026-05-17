using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Contracts.Events;

namespace BuildingBlocks.Contracts.Serialization;

public static class IntegrationEventRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Dictionary<string, Type> EventTypes = new(StringComparer.Ordinal)
    {
        [ProductViewedEvent.TypeName] = typeof(ProductViewedEvent),
        [ProductAddedToCartEvent.TypeName] = typeof(ProductAddedToCartEvent),
        [CheckoutStartedEvent.TypeName] = typeof(CheckoutStartedEvent),
        [OrderCompletedEvent.TypeName] = typeof(OrderCompletedEvent),
        [UserLoggedInEvent.TypeName] = typeof(UserLoggedInEvent)
    };

    public static IReadOnlyDictionary<string, Type> RegisteredTypes => EventTypes;

    public static bool IsRegistered(string eventType) => EventTypes.ContainsKey(eventType);

    public static IntegrationEvent Deserialize(string eventType, string json)
    {
        if (!EventTypes.TryGetValue(eventType, out var clrType))
            throw new InvalidOperationException($"Unknown event type: {eventType}");

        var @event = (IntegrationEvent?)JsonSerializer.Deserialize(json, clrType, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize event type: {eventType}");

        return @event;
    }

    public static string Serialize<TEvent>(TEvent @event) where TEvent : IntegrationEvent =>
        JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions);
}
