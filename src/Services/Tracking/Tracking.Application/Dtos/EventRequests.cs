namespace Tracking.Application.Dtos;

public sealed record ProductViewedRequest(string UserId, string ProductId, string? SessionId, Guid? CorrelationId);
public sealed record ProductAddedToCartRequest(string UserId, string ProductId, int Quantity, Guid? CorrelationId);
public sealed record CheckoutStartedRequest(string UserId, string CartId, decimal TotalAmount, Guid? CorrelationId);
public sealed record OrderCompletedRequest(string UserId, string OrderId, decimal TotalAmount, IReadOnlyList<string> ProductIds, Guid? CorrelationId);
public sealed record UserLoggedInRequest(string UserId, string? IpAddress, string? UserAgent, Guid? CorrelationId);
