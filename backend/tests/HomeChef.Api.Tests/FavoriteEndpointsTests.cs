using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Favorites.Contracts;
using HomeChef.Application.Features.Foods.Contracts;

namespace HomeChef.Api.Tests;

public class FavoriteEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public FavoriteEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private static RegisterRequestPayload NewUser(string role) =>
        new($"fav-{Guid.NewGuid():N}@test.com", role);

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
            firstName = "Favorite",
            lastName = "Tester",
            email = Email,
            password = "Password123",
            role = Role,
        };
    }

    private async Task<(HttpClient ChefClient, Guid ChefProfileId, Guid FoodId)> CreateChefAndFoodAsync()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");
        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new CreateChefProfileRequest
        {
            DisplayName = $"Fav Kitchen {Guid.NewGuid():N}",
            Bio = "Delicious meals for favorites tests.",
            City = "Rawalpindi",
            Area = "Saddar",
            Cuisines = ["Pakistani"],
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        using var profileDoc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(profileDoc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        var createFood = await chefClient.PostAsJsonAsync("/api/chefs/me/foods", new CreateFoodItemRequest
        {
            Name = "Favorite Mutton Karahi",
            Description = "Rich and flavorful dish.",
            Price = 1400.00m,
            Currency = "PKR",
            IsAvailable = true,
        });
        Assert.Equal(HttpStatusCode.Created, createFood.StatusCode);

        using var foodDoc = JsonDocument.Parse(await createFood.Content.ReadAsStringAsync());
        var foodId = Guid.Parse(foodDoc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId, foodId);
    }

    [Fact]
    public async Task ChefFavorite_Lifecycle_AddListRemove()
    {
        var (_, chefProfileId, _) = await CreateChefAndFoodAsync();
        var customerClient = await RegisterAndGetClientAsync("Customer");

        // 1. Add chef to favorites
        var addResponse = await customerClient.PostAsync($"/api/favorites/chefs/{chefProfileId}", null);
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        // 2. Check IDs
        var idsResponse = await customerClient.GetAsync("/api/favorites/ids");
        Assert.Equal(HttpStatusCode.OK, idsResponse.StatusCode);
        var ids = await idsResponse.Content.ReadFromJsonAsync<ApiResponse<UserFavoriteIdsDto>>();
        Assert.Contains(chefProfileId, ids!.Data.ChefIds);

        // 3. List favorite chefs
        var listResponse = await customerClient.GetAsync("/api/favorites/chefs");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listData = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ChefListItemDto>>>();
        Assert.Contains(listData!.Data, c => c.Id == chefProfileId);

        // 4. Remove chef from favorites
        var removeResponse = await customerClient.DeleteAsync($"/api/favorites/chefs/{chefProfileId}");
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        // 5. Verify ID is gone
        var idsAfter = await customerClient.GetFromJsonAsync<ApiResponse<UserFavoriteIdsDto>>("/api/favorites/ids");
        Assert.DoesNotContain(chefProfileId, idsAfter!.Data.ChefIds);
    }

    [Fact]
    public async Task FoodFavorite_Lifecycle_AddListRemove()
    {
        var (_, _, foodId) = await CreateChefAndFoodAsync();
        var customerClient = await RegisterAndGetClientAsync("Customer");

        // 1. Add food to favorites
        var addResponse = await customerClient.PostAsync($"/api/favorites/foods/{foodId}", null);
        Assert.Equal(HttpStatusCode.NoContent, addResponse.StatusCode);

        // 2. Check IDs
        var idsResponse = await customerClient.GetAsync("/api/favorites/ids");
        Assert.Equal(HttpStatusCode.OK, idsResponse.StatusCode);
        var ids = await idsResponse.Content.ReadFromJsonAsync<ApiResponse<UserFavoriteIdsDto>>();
        Assert.Contains(foodId, ids!.Data.FoodIds);

        // 3. List favorite foods
        var listResponse = await customerClient.GetAsync("/api/favorites/foods");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listData = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<FoodListItemDto>>>();
        Assert.Contains(listData!.Data, f => f.Id == foodId);

        // 4. Remove food from favorites
        var removeResponse = await customerClient.DeleteAsync($"/api/favorites/foods/{foodId}");
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        // 5. Verify ID is gone
        var idsAfter = await customerClient.GetFromJsonAsync<ApiResponse<UserFavoriteIdsDto>>("/api/favorites/ids");
        Assert.DoesNotContain(foodId, idsAfter!.Data.FoodIds);
    }

    [Fact]
    public async Task Favorites_Unauthenticated_ReturnsUnauthorized()
    {
        var anonClient = _factory.CreateClient();

        var res1 = await anonClient.PostAsync($"/api/favorites/chefs/{Guid.NewGuid()}", null);
        Assert.Equal(HttpStatusCode.Unauthorized, res1.StatusCode);

        var res2 = await anonClient.GetAsync("/api/favorites/chefs");
        Assert.Equal(HttpStatusCode.Unauthorized, res2.StatusCode);

        var res3 = await anonClient.GetAsync("/api/favorites/ids");
        Assert.Equal(HttpStatusCode.Unauthorized, res3.StatusCode);
    }
}
