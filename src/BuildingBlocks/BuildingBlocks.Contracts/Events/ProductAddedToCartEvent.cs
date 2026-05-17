namespace BuildingBlocks.Contracts.Events;

public sealed record ProductAddedToCartEvent : IntegrationEvent
{
    public const string TypeName = "ProductAddedToCart";
    public override string EventType => TypeName;
    public required string UserId { get; init; }
    public required string ProductId { get; init; }
    public int Quantity { get; init; } = 1;
}
