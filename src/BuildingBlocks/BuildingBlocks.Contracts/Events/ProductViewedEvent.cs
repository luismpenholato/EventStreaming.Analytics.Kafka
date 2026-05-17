namespace BuildingBlocks.Contracts.Events;

public sealed record ProductViewedEvent : IntegrationEvent
{
    public const string TypeName = "ProductViewed";
    public override string EventType => TypeName;
    public required string UserId { get; init; }
    public required string ProductId { get; init; }
    public string? SessionId { get; init; }
}
