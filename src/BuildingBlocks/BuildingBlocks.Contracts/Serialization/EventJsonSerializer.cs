using BuildingBlocks.Contracts.Events;

namespace BuildingBlocks.Contracts.Serialization;

/// <summary>
/// Facade sobre <see cref="IntegrationEventRegistry"/> para compatibilidade.
/// </summary>
public static class EventJsonSerializer
{
    public static string Serialize<TEvent>(TEvent @event) where TEvent : IntegrationEvent =>
        IntegrationEventRegistry.Serialize(@event);

    public static IntegrationEvent Deserialize(string eventType, string json) =>
        IntegrationEventRegistry.Deserialize(eventType, json);
}
