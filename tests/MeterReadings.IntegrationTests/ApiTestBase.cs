using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;

namespace MeterReadings.IntegrationTests;

public abstract class ApiTestBase(ApiFixture fixture)
{
    /// <summary>The seeded site; see SeedData.</summary>
    protected const int SeededSiteId = 1;

    protected ApiFixture Fixture { get; } = fixture;
    protected HttpClient Client { get; } = fixture.CreateClient();

    protected static StringContent Json(string json) => new(json, Encoding.UTF8, "application/json");

    protected static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    /// <summary>Asserts an RFC 7807 problem response with the given status and returns its body.</summary>
    protected static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        response.StatusCode.Should().Be(expected);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await ReadJsonAsync(response);
        problem.GetProperty("status").GetInt32().Should().Be((int)expected);
        return problem;
    }

    /// <summary>Creates a meter with a unique number on the seeded site and returns its id.</summary>
    protected async Task<int> CreateMeterAsync()
    {
        var response = await Client.PostAsJsonAsync(
            $"/api/sites/{SeededSiteId}/meters",
            new { meterNumber = NewMeterNumber() });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await ReadJsonAsync(response)).GetProperty("id").GetInt32();
    }

    protected Task<HttpResponseMessage> PostReadingAsync(int meterId, string timestampUtc, decimal cumulativeKwh) =>
        Client.PostAsJsonAsync($"/api/meters/{meterId}/readings", new { timestampUtc, cumulativeKwh });

    protected static string NewMeterNumber() => $"IT-{Guid.NewGuid():N}";
}
