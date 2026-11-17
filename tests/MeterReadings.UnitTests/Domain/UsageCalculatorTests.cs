using FluentAssertions;
using MeterReadings.Domain.Calculations;
using MeterReadings.UnitTests.TestSupport;

namespace MeterReadings.UnitTests.Domain;

public class UsageCalculatorTests
{
    private static decimal Calculate(double previous, double current, bool isReset = false) =>
        UsageCalculator.Calculate(
            TestReadings.At("2026-11-01T00:00:00Z", (decimal)previous),
            TestReadings.At("2026-11-01T12:00:00Z", (decimal)current, isReset));

    [Theory]
    [InlineData(12500.0, 12521.4, 21.4)]
    [InlineData(0, 1, 1)]
    [InlineData(100, 150.125, 50.125)]
    public void Calculate_NormalIncrease_ReturnsDifference(double previous, double current, double expected)
    {
        Calculate(previous, current).Should().Be((decimal)expected);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12543.7)]
    public void Calculate_EqualReadings_ReturnsZero(double value)
    {
        Calculate(value, value).Should().Be(0m);
    }

    [Theory]
    [InlineData(40, 5)]
    [InlineData(99990, 15)]
    [InlineData(10, 25)]
    [InlineData(500, 0)]
    public void Calculate_ResetReading_ReturnsCurrentValue(double previous, double current)
    {
        Calculate(previous, current, isReset: true).Should().Be((decimal)current);
    }

    [Theory]
    [InlineData(12560.2, 12500.0)]
    [InlineData(99990, 15)]
    [InlineData(0.001, 0)]
    public void Calculate_DecreaseNotFlaggedAsReset_ReturnsZero(double previous, double current)
    {
        Calculate(previous, current).Should().Be(0m);
    }
}
