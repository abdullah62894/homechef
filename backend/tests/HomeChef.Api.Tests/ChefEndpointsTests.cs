using System.Net;
using System.Net.Http.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs.Contracts;

namespace HomeChef.Api.Tests;

public class ChefEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public ChefEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private static RegisterRequestPayload NewUser(string role) =>
        new(
            $"chf-{Guid.NewGuid():N}@test.com",
            role);

    /// <summary>Registers a user of the given role and returns an authenticated client.</summary>
    private async Task<HttpClient> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", NewUser(role).ToJson());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private static CreateChefProfileRequest NewCreateProfile() =>
        new()
        {
            DisplayName = "Test Kitchen",
            Bio = "Fresh home-cooked meals made to order.",
            City = "Karachi",
            Area = "Clifton",
            Cuisines = ["Pakistani", "Bakery", "pakistani"],
        };

    private static UpdateChefProfileRequest NewUpdateProfile() =>
        new()
        {
            DisplayName = "Test Kitchen 2",
            Bio = "Fresh home-cooked meals made to order, plus desserts.",
            City = "Lahore",
            Area = "Gulberg",
            Cuisines = ["Desserts"],
        };

    private sealed record RegisterRequestPayload(string Email, string Role)
    {
        public object ToJson() => new
        {
            firstName = "Integration",
            lastName = "Chef",
            email = Email,
            password = "Password123",
            role = Role,
        };
    }

    [Fact]
    public async Task ChefLifecycle_CreateReadUpdateList()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");

        // Initially no profile.
        var missing = await chefClient.GetAsync("/api/chefs/me");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        // Create.
        var create = await chefClient.PostAsJsonAsync("/api/chefs/me", NewCreateProfile());
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ApiResponse<ChefProfileDto>>();
        Assert.Equal("Test Kitchen", created!.Data.DisplayName);
        Assert.Equal(new[] { "Bakery", "Pakistani" }, created.Data.Cuisines);

        // Create again conflicts.
        var duplicate = await chefClient.PostAsJsonAsync("/api/chefs/me", NewCreateProfile());
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var conflictError = await duplicate.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("CHEF_PROFILE_EXISTS", conflictError!.Error.Code);

        // Read own profile.
        var me = await chefClient.GetAsync("/api/chefs/me");
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
        var meDto = await me.Content.ReadFromJsonAsync<ApiResponse<ChefProfileDto>>();
        Assert.Equal(created.Data.Id, meDto!.Data.Id);

        // Update.
        var update = await chefClient.PutAsJsonAsync("/api/chefs/me", NewUpdateProfile());
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ApiResponse<ChefProfileDto>>();
        Assert.Equal("Test Kitchen 2", updated!.Data.DisplayName);
        Assert.Equal(new[] { "Desserts" }, updated.Data.Cuisines);
        Assert.True(updated.Data.UpdatedAtUtc >= created.Data.UpdatedAtUtc);

        // Public read by id (anonymous client).
        var anonClient = _factory.CreateClient();
        var byId = await anonClient.GetAsync($"/api/chefs/{created.Data.Id}");
        Assert.Equal(HttpStatusCode.OK, byId.StatusCode);

        // Public list (anonymous client) includes the profile with pagination meta.
        var list = await anonClient.GetAsync("/api/chefs?search=Test%20Kitchen&page=1&pageSize=20");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);
        var page = await list.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ChefListItemDto>>>();
        Assert.Contains(page!.Data, c => c.Id == created.Data.Id);
    }

    [Fact]
    public async Task List_PaginationMeta_IsConsistent()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/chefs?page=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var meta = doc.RootElement.GetProperty("meta");
        Assert.Equal(1, meta.GetProperty("page").GetInt32());
        Assert.Equal(10, meta.GetProperty("pageSize").GetInt32());
        Assert.True(meta.TryGetProperty("total", out var total) && total.GetInt32() >= 0);
        Assert.True(meta.TryGetProperty("hasMore", out var hasMore));
        Assert.Equal(1 * 10 < total.GetInt32(), hasMore.GetBoolean());
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsChefProfileNotFound()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/chefs/00000000-0000-0000-0000-000000000000");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("CHEF_PROFILE_NOT_FOUND", error!.Error.Code);
    }

    [Fact]
    public async Task CreateMe_AsCustomer_ReturnsForbidden()
    {
        var customerClient = await RegisterAndGetClientAsync("Customer");

        var response = await customerClient.PostAsJsonAsync("/api/chefs/me", NewCreateProfile());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithoutAuthentication_ReturnsUnauthorized()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/chefs/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}