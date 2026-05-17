namespace BuildingBlocks.Contracts.Events;

public sealed record UserLoggedInEvent : IntegrationEvent
{
    public const string TypeName = "UserLoggedIn";
    public override string EventType => TypeName;
    public required string UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
