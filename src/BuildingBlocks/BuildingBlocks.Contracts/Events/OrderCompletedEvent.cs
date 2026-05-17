namespace BuildingBlocks.Contracts.Events;

public sealed record OrderCompletedEvent : IntegrationEvent
{
    public const string TypeName = "OrderCompleted";
    public override string EventType => TypeName;
    public required string UserId { get; init; }
    public required string OrderId { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<string> ProductIds { get; init; } = [];
}
