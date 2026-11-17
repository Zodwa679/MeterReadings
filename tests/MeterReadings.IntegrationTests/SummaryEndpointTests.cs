using System.Net;
using FluentAssertions;

namespace MeterReadings.IntegrationTests;

[Collection(ApiCollection.Name)]
public class SummaryEndpointTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    private const string FullRange = "fromUtc=2026-11-01T00:00:00Z&toUtc=2026-11-03T00:00:00Z";

    [Fact]
    public async Task GetSummary_SeededMeter1_ReturnsTotalAndDailyBreakdown()
    {
        var response = await Client.GetAsync($"/api/meters/1/summary?{FullRange}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await ReadJsonAsync(response);

        summary.GetProperty("meterId").GetInt32().Should().Be(1);
        summary.GetProperty("fromUtc").GetString().Should().Be("2026-11-01T00:00:00Z");
        summary.GetProperty("toUtc").GetString().Should().Be("2026-11-03T00:00:00Z");
        summary.GetProperty("readingCount").GetInt32().Should().Be(5);
        summary.GetProperty("totalKwh").GetDecimal().Should().Be(81.9m);
        summary.GetProperty("daily").EnumerateArray()
            .Select(d => (d.GetProperty("dateUtc").GetString(), d.GetProperty("kwh").GetDecimal()))
            .Should().Equal(("2026-11-01", 21.4m), ("2026-11-02", 38.8m), ("2026-11-03", 21.7m));
    }

    [Fact]
    public async Task GetSummary_SeededMeter2_HandlesRegisterReset()
    {
        var summary = await ReadJsonAsync(await Client.GetAsync($"/api/meters/2/summary?{FullRange}"));

        summary.GetProperty("totalKwh").GetDecimal().Should().Be(95m);
        summary.GetProperty("daily").EnumerateArray().Select(d => d.GetProperty("kwh").GetDecimal())
            .Should().Equal(40m, 40m, 15m);
    }

    [Fact]
    public async Task GetSummary_RangeGivenWithOffset_IsConvertedToUtc()
    {
        // 02:00+02:00 is 00:00Z, so the first seeded reading sits exactly on the inclusive boundary.
        var response = await Client.GetAsync(
            "/api/meters/1/summary?fromUtc=2026-11-01T02:00:00%2B02:00&toUtc=2026-11-01T12:00:00Z");

        var summary = await ReadJsonAsync(response);
        summary.GetProperty("fromUtc").GetString().Should().Be("2026-11-01T00:00:00Z");
        summary.GetProperty("readingCount").GetInt32().Should().Be(2);
        summary.GetProperty("totalKwh").GetDecimal().Should().Be(21.4m);
    }

    [Fact]
    public async Task GetSummary_FewerThanTwoReadingsInRange_ReturnsZero()
    {
        var response = await Client.GetAsync(
            "/api/meters/1/summary?fromUtc=2026-11-01T00:00:00Z&toUtc=2026-11-01T00:00:00Z");

        var summary = await ReadJsonAsync(response);
        summary.GetProperty("readingCount").GetInt32().Should().Be(1);
        summary.GetProperty("totalKwh").GetDecimal().Should().Be(0m);
        summary.GetProperty("daily").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetSummary_UnexplainedDecrease_IsExcludedFromTotal()
    {
        var meterId = await CreateMeterAsync();
        await PostReadingAsync(meterId, "2026-11-01T00:00:00Z", 500m);
        await PostReadingAsync(meterId, "2026-11-01T12:00:00Z", 520m);
        await PostReadingAsync(meterId, "2026-11-02T00:00:00Z", 300m);
        await PostReadingAsync(meterId, "2026-11-02T12:00:00Z", 310m);

        var summary = await ReadJsonAsync(await Client.GetAsync($"/api/meters/{meterId}/summary?{FullRange}"));

        summary.GetProperty("totalKwh").GetDecimal().Should().Be(30m);
    }

    [Theory]
    [InlineData("", "fromUtc")]
    [InlineData("fromUtc=2026-11-01T00:00:00Z", "toUtc")]
    [InlineData("fromUtc=2026-11-01&toUtc=2026-11-03T00:00:00Z", "fromUtc")]
    [InlineData("fromUtc=2026-11-03T00:00:00Z&toUtc=2026-11-01T00:00:00Z", "fromUtc")]
    public async Task GetSummary_MissingOrInvalidRange_Returns400ValidationProblem(string query, string expectedErrorKey)
    {
        var response = await Client.GetAsync($"/api/meters/1/summary?{query}");

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        problem.GetProperty("errors").TryGetProperty(expectedErrorKey, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetSummary_UnknownMeter_Returns404Problem()
    {
        await ReadProblemAsync(await Client.GetAsync($"/api/meters/999999/summary?{FullRange}"), HttpStatusCode.NotFound);
    }
}
