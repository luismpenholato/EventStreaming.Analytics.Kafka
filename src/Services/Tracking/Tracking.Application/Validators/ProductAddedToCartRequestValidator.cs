using FluentValidation;
using Tracking.Application.Dtos;

namespace Tracking.Application.Validators;

public sealed class ProductAddedToCartRequestValidator : AbstractValidator<ProductAddedToCartRequest>
{
    public ProductAddedToCartRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
