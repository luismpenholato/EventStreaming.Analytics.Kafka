using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Contracts.Topics;
using BuildingBlocks.Observability;
using Microsoft.AspNetCore.Mvc;
using Tracking.Application.Abstractions;
using Tracking.Application.Dtos;

namespace Tracking.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(
    IEventTrackingService trackingService,
    IRequestValidator requestValidator) : ControllerBase
{
    [HttpPost("product-viewed")]
    public async Task<IActionResult> ProductViewed(
        [FromBody] ProductViewedRequest request,
        CancellationToken cancellationToken)
    {
        await requestValidator.ValidateAsync(request, cancellationToken);
        var @event = new ProductViewedEvent
        {
            UserId = request.UserId,
            ProductId = request.ProductId,
            SessionId = request.SessionId,
            CorrelationId = GetCorrelationId(request.CorrelationId)
        };
        return await TrackAsync(@event, KafkaTopics.UserActivityEvents, cancellationToken);
    }

    [HttpPost("product-added-to-cart")]
    public async Task<IActionResult> ProductAddedToCart(
        [FromBody] ProductAddedToCartRequest request,
        CancellationToken cancellationToken)
    {
        await requestValidator.ValidateAsync(request, cancellationToken);
        var @event = new ProductAddedToCartEvent
        {
            UserId = request.UserId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            CorrelationId = GetCorrelationId(request.CorrelationId)
        };
        return await TrackAsync(@event, KafkaTopics.UserActivityEvents, cancellationToken);
    }

    [HttpPost("checkout-started")]
    public async Task<IActionResult> CheckoutStarted(
        [FromBody] CheckoutStartedRequest request,
        CancellationToken cancellationToken)
    {
        await requestValidator.ValidateAsync(request, cancellationToken);
        var @event = new CheckoutStartedEvent
        {
            UserId = request.UserId,
            CartId = request.CartId,
            TotalAmount = request.TotalAmount,
            CorrelationId = GetCorrelationId(request.CorrelationId)
        };
        return await TrackAsync(@event, KafkaTopics.UserActivityEvents, cancellationToken);
    }

    [HttpPost("order-completed")]
    public async Task<IActionResult> OrderCompleted(
        [FromBody] OrderCompletedRequest request,
        CancellationToken cancellationToken)
    {
        await requestValidator.ValidateAsync(request, cancellationToken);
        var @event = new OrderCompletedEvent
        {
            UserId = request.UserId,
            OrderId = request.OrderId,
            TotalAmount = request.TotalAmount,
            ProductIds = request.ProductIds,
            CorrelationId = GetCorrelationId(request.CorrelationId)
        };
        return await TrackAsync(@event, KafkaTopics.OrderEvents, cancellationToken);
    }

    [HttpPost("user-logged-in")]
    public async Task<IActionResult> UserLoggedIn(
        [FromBody] UserLoggedInRequest request,
        CancellationToken cancellationToken)
    {
        await requestValidator.ValidateAsync(request, cancellationToken);
        var @event = new UserLoggedInEvent
        {
            UserId = request.UserId,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            CorrelationId = GetCorrelationId(request.CorrelationId)
        };
        return await TrackAsync(@event, KafkaTopics.UserActivityEvents, cancellationToken);
    }

    private Guid GetCorrelationId(Guid? requestCorrelationId) =>
        requestCorrelationId
        ?? (HttpContext.Items[CorrelationIdMiddleware.ItemKey] as Guid?)
        ?? Guid.NewGuid();

    private async Task<IActionResult> TrackAsync<TEvent>(
        TEvent @event,
        string topic,
        CancellationToken cancellationToken) where TEvent : IntegrationEvent
    {
        var result = await trackingService.TrackAsync(@event, topic, cancellationToken);
        if (!result.IsSuccess)
            return StatusCode(503, new { error = result.Error });

        return Accepted(new
        {
            eventId = result.Value,
            correlationId = @event.CorrelationId,
            eventType = @event.EventType
        });
    }
}
