using FluentValidation;
using Tracking.Application.Dtos;

namespace Tracking.Application.Validators;

public sealed class CheckoutStartedRequestValidator : AbstractValidator<CheckoutStartedRequest>
{
    public CheckoutStartedRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.CartId).NotEmpty();
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
    }
}
