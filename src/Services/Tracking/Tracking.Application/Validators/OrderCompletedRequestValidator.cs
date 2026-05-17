using FluentValidation;
using Tracking.Application.Dtos;

namespace Tracking.Application.Validators;

public sealed class OrderCompletedRequestValidator : AbstractValidator<OrderCompletedRequest>
{
    public OrderCompletedRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ProductIds).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
    }
}
