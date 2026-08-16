using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs.Contracts;

namespace HomeChef.Api.Tests;

public class SearchEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public SearchEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private static RegisterRequestPayload NewUser(string role) =>
        new($"srch-{Guid.NewGuid():N}@test.com", role);

    private async Task<HttpClient> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", NewUser(role).ToJson());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private sealed record RegisterRequestPayload(string Email, string Role)
    {
        public object ToJson() => new
        {
            firstName = "Search",
            lastName = "Tester",
            email = Email,
            password = "Password123",
            role = Role,
        };
    }

    [Fact]
    public async Task Search_ReturnsResults()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/search?q=test&page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("data").TryGetProperty("chefs", out _));
        Assert.True(doc.RootElement.GetProperty("data").TryGetProperty("foods", out _));
        Assert.True(doc.RootElement.GetProperty("data").TryGetProperty("totalChefs", out _));
        Assert.True(doc.RootElement.GetProperty("data").TryGetProperty("totalFoods", out _));
    }

    [Fact]
    public async Task Search_WithTypeFilter_OnlyReturnsMatchingType()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/search?type=chefs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(0, data.GetProperty("totalFoods").GetInt32());
    }

    [Fact]
    public async Task Locations_ReturnsDirectory()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/locations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.GetProperty("data").TryGetProperty("cities", out var cities));
        Assert.Equal(JsonValueKind.Array, cities.ValueKind);
    }

    [Fact]
    public async Task Locations_City_NotFound_Returns404()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/locations/NonExistentCity12345");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Search_WithCityFilter_ReturnsFilteredResults()
    {
        // Create a chef in a specific city
        var chefClient = await RegisterAndGetClientAsync("Chef");
        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = "Search Test Chef",
            bio = "Home-cooked meals for search test.",
            city = "Multan",
            area = "Cantt",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/search?city=Multan&type=chefs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var totalChefs = doc.RootElement.GetProperty("data").GetProperty("totalChefs").GetInt32();
        Assert.True(totalChefs >= 1);
    }

    [Fact]
    public async Task Locations_CityArea_ReturnsChefs()
    {
        // Create a chef in a specific city/area
        var chefClient = await RegisterAndGetClientAsync("Chef");
        await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = "Area Test Chef",
            bio = "Home-cooked meals for location test.",
            city = "Faisalabad",
            area = "PeopleColony",
            cuisines = new[] { "Pakistani" },
        });

        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/locations/Faisalabad/PeopleColony");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.GetProperty("data");
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }

    [Fact]
    public async Task Chefs_WithSearchFilter_ReturnsFilteredResults()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/chefs?search=NonExistent999");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = doc.RootElement.GetProperty("data");
        Assert.Equal(0, data.GetArrayLength());
    }

    [Fact]
    public async Task Foods_WithCityFilter_ReturnsOk()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/foods?city=Karachi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
