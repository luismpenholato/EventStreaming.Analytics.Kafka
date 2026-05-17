using Analytics.Domain.Metrics;
using FluentAssertions;

namespace Analytics.UnitTests.Metrics;

public sealed class ConversionRateTests
{
    [Theory]
    [InlineData(100, 5, 5.00)]
    [InlineData(0, 10, 0)]
    [InlineData(50, 1, 2.00)]
    public void Should_calculate_conversion_rate(long views, long orders, decimal expected) =>
        ConversionRateCalculator.Calculate(views, orders).Should().Be(expected);
}
