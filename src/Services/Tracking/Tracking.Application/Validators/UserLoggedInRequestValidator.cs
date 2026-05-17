using FluentValidation;
using Tracking.Application.Dtos;

namespace Tracking.Application.Validators;

public sealed class UserLoggedInRequestValidator : AbstractValidator<UserLoggedInRequest>
{
    public UserLoggedInRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
