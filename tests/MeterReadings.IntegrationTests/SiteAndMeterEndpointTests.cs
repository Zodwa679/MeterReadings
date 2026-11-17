using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace MeterReadings.IntegrationTests;

[Collection(ApiCollection.Name)]
public class SiteAndMeterEndpointTests(ApiFixture fixture) : ApiTestBase(fixture)
{
    [Fact]
    public async Task PostSite_WithoutStatus_Returns201AndDefaultsToActive()
    {
        var response = await Client.PostAsJsonAsync("/api/sites", new { address = " 12 Main Road ", siteType = "Residential" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var site = await ReadJsonAsync(response);
        var id = site.GetProperty("id").GetInt32();

        response.Headers.Location!.AbsolutePath.Should().Be($"/api/sites/{id}");
        site.GetProperty("address").GetString().Should().Be("12 Main Road");
        site.GetProperty("siteType").GetString().Should().Be("Residential");
        site.GetProperty("status").GetString().Should().Be("Active");
        site.GetProperty("createdAt").GetString().Should().EndWith("Z");

        (await Client.GetAsync($"/api/sites/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostSite_MissingAddress_Returns400WithAddressError()
    {
        var response = await Client.PostAsync("/api/sites", Json("{}"));

        var problem = await ReadProblemAsync(response, HttpStatusCode.BadRequest);
        var errors = problem.GetProperty("errors");
        errors.TryGetProperty("Address", out _).Should().BeTrue();
        errors.TryGetProperty("SiteType", out _).Should().BeFalse("siteType is optional");
        errors.TryGetProperty("Status", out _).Should().BeFalse("status is optional");
    }

    [Fact]
    public async Task PostSite_WithOnlyAddress_Returns201WithDefaults()
    {
        var response = await Client.PostAsJsonAsync("/api/sites", new { address = "1 Main Road" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var site = await ReadJsonAsync(response);
        site.GetProperty("siteType").GetString().Should().Be("Unspecified");
        site.GetProperty("status").GetString().Should().Be("Active");
    }

    [Fact]
    public async Task PostSite_UnknownEnumValue_Returns400()
    {
        var response = await Client.PostAsync("/api/sites", Json("""{"address":"1 Main Road","siteType":"Castle"}"""));

        var errors = (await ReadProblemAsync(response, HttpStatusCode.BadRequest)).GetProperty("errors");
        errors.TryGetProperty("$.siteType", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetSites_ReturnsTheSeededSite()
    {
        var sites = await ReadJsonAsync(await Client.GetAsync("/api/sites"));

        sites.EnumerateArray().Select(s => s.GetProperty("id").GetInt32()).Should().Contain(SeededSiteId);
    }

    [Fact]
    public async Task GetSite_Unknown_Returns404Problem()
    {
        var problem = await ReadProblemAsync(await Client.GetAsync("/api/sites/999999"), HttpStatusCode.NotFound);

        problem.GetProperty("detail").GetString().Should().Be("Site 999999 was not found.");
    }

    [Fact]
    public async Task PostMeter_WithOnlyMeterNumber_Returns201WithDefaults()
    {
        var meterNumber = NewMeterNumber();

        var response = await Client.PostAsJsonAsync($"/api/sites/{SeededSiteId}/meters", new { meterNumber });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var meter = await ReadJsonAsync(response);
        response.Headers.Location!.AbsolutePath.Should().Be($"/api/meters/{meter.GetProperty("id").GetInt32()}");
        meter.GetProperty("siteId").GetInt32().Should().Be(SeededSiteId);
        meter.GetProperty("meterType").GetString().Should().Be("Electricity");
        meter.GetProperty("status").GetString().Should().Be("Active");

        var listed = await ReadJsonAsync(await Client.GetAsync($"/api/sites/{SeededSiteId}/meters"));
        listed.EnumerateArray().Select(m => m.GetProperty("meterNumber").GetString()).Should().Contain(meterNumber);
    }

    [Fact]
    public async Task PostMeter_UnknownSite_Returns404Problem()
    {
        var response = await Client.PostAsJsonAsync("/api/sites/999999/meters", new { meterNumber = NewMeterNumber() });

        await ReadProblemAsync(response, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostMeter_DuplicateMeterNumber_Returns409Problem()
    {
        var meterNumber = NewMeterNumber();
        await Client.PostAsJsonAsync($"/api/sites/{SeededSiteId}/meters", new { meterNumber });

        var response = await Client.PostAsJsonAsync($"/api/sites/{SeededSiteId}/meters", new { meterNumber });

        var problem = await ReadProblemAsync(response, HttpStatusCode.Conflict);
        problem.GetProperty("detail").GetString().Should().Be("A record with the same unique value already exists.");
    }

    [Fact]
    public async Task GetMeters_UnknownSite_Returns404Problem()
    {
        await ReadProblemAsync(await Client.GetAsync("/api/sites/999999/meters"), HttpStatusCode.NotFound);
    }
}
