namespace BuildingBlocks.Contracts.Events;

public sealed record CheckoutStartedEvent : IntegrationEvent
{
    public const string TypeName = "CheckoutStarted";
    public override string EventType => TypeName;
    public required string UserId { get; init; }
    public required string CartId { get; init; }
    public decimal TotalAmount { get; init; }
}
