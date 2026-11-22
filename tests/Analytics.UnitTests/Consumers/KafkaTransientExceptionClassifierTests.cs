using BuildingBlocks.Kafka.Consumers;
using FluentAssertions;

namespace Analytics.UnitTests.Consumers;

public sealed class KafkaTransientExceptionClassifierTests
{
    [Fact]
    public void IsTransient_TimeoutException_ReturnsTrue()
    {
        KafkaTransientExceptionClassifier.IsTransient(new TimeoutException("slow"))
            .Should().BeTrue();
    }

    [Fact]
    public void IsTransient_InvalidOperationException_ReturnsFalse()
    {
        KafkaTransientExceptionClassifier.IsTransient(new InvalidOperationException("bad state"))
            .Should().BeFalse();
    }

    [Fact]
    public void IsTransient_WrappedTimeout_ReturnsTrue()
    {
        var wrapped = new Exception("outer", new TimeoutException("inner"));

        KafkaTransientExceptionClassifier.IsTransient(wrapped).Should().BeTrue();
    }

    [Fact]
    public void IsTransient_DbUpdateStyleException_ReturnsTrue()
    {
        KafkaTransientExceptionClassifier.IsTransient(new DbUpdateExceptionStub("conflict"))
            .Should().BeTrue();
    }

    [Fact]
    public void IsTransient_NpgsqlStyleException_ReturnsTrue()
    {
        KafkaTransientExceptionClassifier.IsTransient(new NpgsqlExceptionStub("connection"))
            .Should().BeTrue();
    }

    [Fact]
    public void IsTransient_GenericException_ReturnsFalse()
    {
        KafkaTransientExceptionClassifier.IsTransient(new Exception("boom"))
            .Should().BeFalse();
    }

    private sealed class DbUpdateExceptionStub(string message) : Exception(message);

    private sealed class NpgsqlExceptionStub(string message) : Exception(message);
}
