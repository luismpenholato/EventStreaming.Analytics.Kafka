using FluentValidation;
using Tracking.Application.Dtos;

namespace Tracking.Application.Validators;

public sealed class ProductViewedRequestValidator : AbstractValidator<ProductViewedRequest>
{
    public ProductViewedRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProductId).NotEmpty().MaximumLength(100);
    }
}
