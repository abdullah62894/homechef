using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;

namespace HomeChef.Api.Tests;

public class HealthEndpointTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(HomeChefApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk_WhenDatabaseIsReachable()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UnknownApiRoute_ReturnsNotFoundErrorContract()
    {
        var response = await _client.GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(body);
        Assert.Equal("NOT_FOUND", body!.Error.Code);
    }

    [Fact]
    public async Task OpenApiDocument_IsAvailable_InDevelopment()
    {
        var response = await _client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var title = doc.RootElement.GetProperty("info").GetProperty("title").GetString();
        Assert.Contains("HomeChef", title, StringComparison.OrdinalIgnoreCase);
    }
}