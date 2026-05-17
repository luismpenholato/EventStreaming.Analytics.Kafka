using FluentAssertions;
using Tracking.Application.Dtos;
using Tracking.Application.Validators;

namespace Tracking.UnitTests.Validators;

public sealed class ProductViewedRequestValidatorTests
{
    private readonly ProductViewedRequestValidator _validator = new();

    [Fact]
    public void Should_fail_when_user_id_is_empty()
    {
        var request = new ProductViewedRequest("", "prod-1", null, null);
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_pass_for_valid_request()
    {
        var request = new ProductViewedRequest("user-1", "prod-1", "session-1", Guid.NewGuid());
        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }
}
