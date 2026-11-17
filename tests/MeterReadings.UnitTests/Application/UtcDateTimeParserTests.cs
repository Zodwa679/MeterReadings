using FluentAssertions;
using MeterReadings.Application.Common;

namespace MeterReadings.UnitTests.Application;

public class UtcDateTimeParserTests
{
    [Theory]
    [InlineData("2026-11-01T08:00:00Z")]
    [InlineData("2026-11-01T08:00:00.000Z")]
    public void TryParse_ValueEndingInZ_ReturnsUtc(string value)
    {
        var ok = UtcDateTimeParser.TryParse(value, out var utc);

        ok.Should().BeTrue();
        utc.Kind.Should().Be(DateTimeKind.Utc);
        utc.Should().Be(new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData("2026-11-01T10:00:00+02:00")]
    [InlineData("2026-11-01T03:00:00-05:00")]
    [InlineData("2026-11-01T08:00:00+00:00")]
    public void TryParse_ValueWithOffset_ConvertsToUtc(string value)
    {
        var ok = UtcDateTimeParser.TryParse(value, out var utc);

        ok.Should().BeTrue();
        utc.Kind.Should().Be(DateTimeKind.Utc);
        utc.Should().Be(new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc));
    }

    [Theory]
    [InlineData("2026-11-01T08:00:00")]
    [InlineData("2026-11-01")]
    [InlineData("2026-11-01 08:00")]
    public void TryParse_ValueWithoutZOrOffset_ReturnsFalse(string value)
    {
        UtcDateTimeParser.TryParse(value, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-dateZ")]
    [InlineData("2026-13-45T08:00:00Z")]
    [InlineData("yesterday+02:00")]
    public void TryParse_NullEmptyOrUnparseable_ReturnsFalse(string? value)
    {
        UtcDateTimeParser.TryParse(value, out _).Should().BeFalse();
    }
}
