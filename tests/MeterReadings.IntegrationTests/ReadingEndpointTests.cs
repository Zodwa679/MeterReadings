using System.Net;
using FluentAssertions;

namespace MeterReadings.IntegrationTests;

[Collection(ApiCollection.Name)]
public class ReadingEndpointTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task PostReading_Valid_Returns201WithUtcTimestamp()
    {
        var meterId = await CreateMeterAsync();

        var response = await PostReadingAsync(meterId, "2026-11-01T08:00:00Z", 12543.7m);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.AbsolutePath.Should().Be($"/api/meters/{meterId}/readings");
        var reading = await ReadJsonAsync(response);
        reading.GetProperty("meterId").GetInt32().Should().Be(meterId);
        reading.GetProperty("timestampUtc").GetString().Should().Be("2026-11-01T08:00:00Z");
        reading.GetProperty("cumulativeKwh").GetDecimal().Should().Be(12543.7m);
        reading.GetProperty("isReset").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task PostReading_TimestampWithOffset_IsStoredAndReturnedAsUtc()
    {
        var meterId = await CreateMeterAsync();

        var response = await PostReadingAsync(meterId, "2026-11-01T10:00:00+02:00", 1m);

        (await ReadJsonAsync(response)).GetProperty("timestampUtc").GetString().Should().Be("2026-11-01T08:00:00Z");
    }

    [Fact]
    public async Task PostReading_SameInstantTwice_Returns409Problem()
    {
        var meterId = await CreateMeterAsync();
        await PostReadingAsync(meterId, "2026-11-01T08:00:00Z", 100m);

        // 10:00+02:00 is the same instant as 08:00Z.
        var response = await PostReadingAsync(meterId, "2026-11-01T10:00:00+02:00", 120m);

        var problem = await ReadProblemAsync(response, HttpStatusCode.Conflict);
        problem.GetProperty("detail").GetString().Should().Be("A record with the same unique value already exists.");
    }

    [Fact]
    public async Task PostReading_SameTimestampOnAnotherMeter_Returns201()
    {
        var first = await CreateMeterAsync();
        var second = await CreateMeterAsync();
        await PostReadingAsync(first, "2026-11-01T08:00:00Z", 100m);

        var response = await PostReadingAsync(second, "2026-11-01T08:00:00Z", 100m);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostReading_TimestampWithoutZone_Returns400WithExplanation()
    {
        var meterId = await CreateMeterAsync();

        var response = await PostReadingAsync(meterId, "2026-11-01T08:00:00", 1m);

        var errors = (await ReadProblemAsync(response, HttpStatusCode.BadRequest)).GetProperty("errors");
        // The framework keys JSON read errors by JSON path.
        errors.GetProperty("$.timestampUtc")[0].GetString().Should().Contain("is not a valid UTC timestamp");
    }

    [Fact]
    public async Task PostReading_MalformedJson_Returns400Problem()
    {
        var meterId = await CreateMeterAsync();

        var response = await Client.PostAsync($"/api/meters/{meterId}/readings", Json("{ not json"));

        var errors = (await ReadProblemAsync(response, HttpStatusCode.BadRequest)).GetProperty("errors");
        errors.TryGetProperty("$", out _).Should().BeTrue();
    }

    [Fact]
    public async Task PostReading_EmptyBody_Returns400Problem()
    {
        var meterId = await CreateMeterAsync();

        var response = await Client.PostAsync($"/api/meters/{meterId}/readings", Json(""));

        var errors = (await ReadProblemAsync(response, HttpStatusCode.BadRequest)).GetProperty("errors");
        errors.GetRawText().Should().Contain("A non-empty request body is required.");
    }

    [Fact]
    public async Task PostReading_NegativeKwhAndMissingTimestamp_Returns400WithBothErrors()
    {
        var meterId = await CreateMeterAsync();

        var response = await Client.PostAsync($"/api/meters/{meterId}/readings", Json("""{"cumulativeKwh":-1}"""));

        var errors = (await ReadProblemAsync(response, HttpStatusCode.BadRequest)).GetProperty("errors");
        errors.TryGetProperty("TimestampUtc", out _).Should().BeTrue();
        errors.TryGetProperty("CumulativeKwh", out _).Should().BeTrue();
    }

    [Fact]
    public async Task PostReading_UnknownMeter_Returns404Problem()
    {
        var response = await PostReadingAsync(999999, "2026-11-01T08:00:00Z", 1m);

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReadings_WithRange_IsInclusiveAndOrderedByTimestamp()
    {
        var meterId = await CreateMeterAsync();
        await PostReadingAsync(meterId, "2026-11-03T00:00:00Z", 40m);
        await PostReadingAsync(meterId, "2026-11-01T00:00:00Z", 10m);
        await PostReadingAsync(meterId, "2026-11-02T00:00:00Z", 30m);
        await PostReadingAsync(meterId, "2026-11-01T12:00:00Z", 20m);

        var all = await ReadJsonAsync(await Client.GetAsync($"/api/meters/{meterId}/readings"));
        var ranged = await ReadJsonAsync(await Client.GetAsync(
            $"/api/meters/{meterId}/readings?fromUtc=2026-11-01T12:00:00Z&toUtc=2026-11-02T00:00:00Z"));
        var fromOnly = await ReadJsonAsync(await Client.GetAsync(
            $"/api/meters/{meterId}/readings?fromUtc=2026-11-02T00:00:00Z"));

        Kwh(all).Should().Equal(10m, 20m, 30m, 40m);
        Kwh(ranged).Should().Equal(20m, 30m);
        Kwh(fromOnly).Should().Equal(30m, 40m);
    }

    [Theory]
    [InlineData("fromUtc=2026-11-01T00:00:00", "fromUtc")] // no Z or offset
    [InlineData("toUtc=yesterday", "toUtc")]
    [InlineData("fromUtc=2026-11-03T00:00:00Z&toUtc=2026-11-01T00:00:00Z", "fromUtc")] // from > to
    public async Task GetReadings_InvalidRange_Returns400ValidationProblem(string query, string expectedErrorKey)
    {
        var response = await Client.GetAsync($"/api/meters/1/readings?{query}");

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        problem.GetProperty("errors").TryGetProperty(expectedErrorKey, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetReadings_UnknownMeter_Returns404Problem()
    {
        await ReadProblemAsync(await Client.GetAsync("/api/meters/999999/readings"), HttpStatusCode.NotFound);
    }

    private static IEnumerable<decimal> Kwh(System.Text.Json.JsonElement readings) =>
        readings.EnumerateArray().Select(r => r.GetProperty("cumulativeKwh").GetDecimal());
}
