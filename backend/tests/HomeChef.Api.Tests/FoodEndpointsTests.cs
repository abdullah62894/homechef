using System.Net;
using System.Net.Http.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Foods.Contracts;

namespace HomeChef.Api.Tests;

public class FoodEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public FoodEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private static RegisterRequestPayload NewUser(string role) =>
        new($"fd-{Guid.NewGuid():N}@test.com", role);

    private async Task<HttpClient> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", NewUser(role).ToJson());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private static CreateChefProfileRequest NewChefProfile() =>
        new()
        {
            DisplayName = "Maryam's Homemade Food",
            Bio = "Authentic home-cooked meals made with love.",
            City = "Islamabad",
            Area = "F-10",
            Cuisines = ["Pakistani", "Rice"],
        };

    private sealed record RegisterRequestPayload(string Email, string Role)
    {
        public object ToJson() => new
        {
            firstName = "Chef",
            lastName = "Test",
            email = Email,
            password = "Password123",
            role = Role,
        };
    }

    [Fact]
    public async Task Categories_ReturnsSeededCategories()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.GetAsync("/api/foods/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<FoodCategoryDto>>>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Data);
        Assert.Contains(result.Data, c => c.Slug == "rice-biryani");
        Assert.Contains(result.Data, c => c.Slug == "main-course");
    }

    [Fact]
    public async Task FoodLifecycle_CreateReadUpdateAvailabilityDelete()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");

        // 1. Create chef profile first
        var profileRes = await chefClient.PostAsJsonAsync("/api/chefs/me", NewChefProfile());
        Assert.Equal(HttpStatusCode.Created, profileRes.StatusCode);
        var profile = (await profileRes.Content.ReadFromJsonAsync<ApiResponse<ChefProfileDto>>())!.Data;

        // 2. Fetch categories to get a valid category ID
        var anonClient = _factory.CreateClient();
        var catsRes = await anonClient.GetAsync("/api/foods/categories");
        var catsEnvelope = await catsRes.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<FoodCategoryDto>>>();
        var biryaniCategory = catsEnvelope!.Data.First(c => c.Slug == "rice-biryani");

        // 3. Create food item
        var createRequest = new CreateFoodItemRequest
        {
            Name = "Special Chicken Biryani",
            Description = "Long-grain basmati rice cooked with spiced chicken and potatoes.",
            Price = 650.00m,
            Currency = "PKR",
            CategoryId = biryaniCategory.Id,
            IsAvailable = true,
            PreparationTimeMinutes = 45,
        };

        var createRes = await chefClient.PostAsJsonAsync("/api/chefs/me/foods", createRequest);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var created = (await createRes.Content.ReadFromJsonAsync<ApiResponse<FoodItemDto>>())!.Data;
        Assert.Equal("Special Chicken Biryani", created.Name);
        Assert.Equal(650.00m, created.Price);
        Assert.Equal(profile.Id, created.ChefProfileId);
        Assert.Equal("Maryam's Homemade Food", created.ChefDisplayName);
        Assert.Equal("Rice & Biryani", created.CategoryName);
        Assert.True(created.IsAvailable);

        // 4. Get by ID publicly
        var getRes = await anonClient.GetAsync($"/api/foods/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        var fetched = (await getRes.Content.ReadFromJsonAsync<ApiResponse<FoodItemDto>>())!.Data;
        Assert.Equal(created.Id, fetched.Id);

        // 5. Update food item
        var updateRequest = new UpdateFoodItemRequest
        {
            Name = "Special Chicken Biryani (Family Pack)",
            Description = "Long-grain basmati rice cooked with spiced chicken, serves 3-4.",
            Price = 1200.00m,
            Currency = "PKR",
            CategoryId = biryaniCategory.Id,
            IsAvailable = true,
            PreparationTimeMinutes = 60,
        };
        var updateRes = await chefClient.PutAsJsonAsync($"/api/chefs/me/foods/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);
        var updated = (await updateRes.Content.ReadFromJsonAsync<ApiResponse<FoodItemDto>>())!.Data;
        Assert.Equal("Special Chicken Biryani (Family Pack)", updated.Name);
        Assert.Equal(1200.00m, updated.Price);

        // 6. Toggle availability
        var toggleRes = await chefClient.PatchAsJsonAsync(
            $"/api/chefs/me/foods/{created.Id}/availability",
            new SetFoodAvailabilityRequest { IsAvailable = false });
        Assert.Equal(HttpStatusCode.OK, toggleRes.StatusCode);
        var toggled = (await toggleRes.Content.ReadFromJsonAsync<ApiResponse<FoodItemDto>>())!.Data;
        Assert.False(toggled.IsAvailable);

        // 7. List chef's food items
        var chefFoodsRes = await anonClient.GetAsync($"/api/chefs/{profile.Id}/foods");
        Assert.Equal(HttpStatusCode.OK, chefFoodsRes.StatusCode);
        var chefFoods = (await chefFoodsRes.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<FoodListItemDto>>>())!.Data;
        Assert.Contains(chefFoods, f => f.Id == created.Id);

        // 8. List public foods with search filter
        var searchRes = await anonClient.GetAsync("/api/foods?search=Biryani");
        Assert.Equal(HttpStatusCode.OK, searchRes.StatusCode);
        var searchResults = (await searchRes.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<FoodListItemDto>>>())!.Data;
        Assert.Contains(searchResults, f => f.Id == created.Id);

        // 9. Delete food item
        var deleteRes = await chefClient.DeleteAsync($"/api/chefs/me/foods/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        // 10. Verify food item is deleted
        var notFoundRes = await anonClient.GetAsync($"/api/foods/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, notFoundRes.StatusCode);
        var notFoundError = await notFoundRes.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("FOOD_ITEM_NOT_FOUND", notFoundError!.Error.Code);
    }

    [Fact]
    public async Task CreateFood_WithoutChefProfile_ReturnsChefProfileRequired()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");

        var createRequest = new CreateFoodItemRequest
        {
            Name = "Mutton Karahi",
            Description = "Fresh mutton cooked in tomato and green chilies.",
            Price = 1500.00m,
        };

        var response = await chefClient.PostAsJsonAsync("/api/chefs/me/foods", createRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("CHEF_PROFILE_REQUIRED", error!.Error.Code);
    }

    [Fact]
    public async Task CreateFood_AsCustomer_ReturnsForbidden()
    {
        var customerClient = await RegisterAndGetClientAsync("Customer");

        var createRequest = new CreateFoodItemRequest
        {
            Name = "Custard Dessert",
            Description = "Sweet fruit custard.",
            Price = 300.00m,
        };

        var response = await customerClient.PostAsJsonAsync("/api/chefs/me/foods", createRequest);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ModifyFood_ByAnotherChef_ReturnsForbidden()
    {
        // Chef A creates profile and food
        var chefAClient = await RegisterAndGetClientAsync("Chef");
        await chefAClient.PostAsJsonAsync("/api/chefs/me", NewChefProfile());
        var createRes = await chefAClient.PostAsJsonAsync("/api/chefs/me/foods", new CreateFoodItemRequest
        {
            Name = "Chef A Dish",
            Description = "Special dish made by Chef A.",
            Price = 500.00m,
        });
        var food = (await createRes.Content.ReadFromJsonAsync<ApiResponse<FoodItemDto>>())!.Data;

        // Chef B creates profile
        var chefBClient = await RegisterAndGetClientAsync("Chef");
        await chefBClient.PostAsJsonAsync("/api/chefs/me", new CreateChefProfileRequest
        {
            DisplayName = "Chef B Kitchen",
            Bio = "Another kitchen bio.",
            City = "Lahore",
        });

        // Chef B tries to update Chef A's food
        var updateRes = await chefBClient.PutAsJsonAsync($"/api/chefs/me/foods/{food.Id}", new UpdateFoodItemRequest
        {
            Name = "Hacked Dish",
            Description = "Should not be updated.",
            Price = 100.00m,
        });

        Assert.Equal(HttpStatusCode.Forbidden, updateRes.StatusCode);
        var updateErr = await updateRes.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("FOOD_ITEM_FORBIDDEN", updateErr!.Error.Code);

        // Chef B tries to delete Chef A's food
        var deleteRes = await chefBClient.DeleteAsync($"/api/chefs/me/foods/{food.Id}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteRes.StatusCode);
        var deleteErr = await deleteRes.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("FOOD_ITEM_FORBIDDEN", deleteErr!.Error.Code);
    }

    [Fact]
    public async Task CreateFood_InvalidPrice_ReturnsValidationError()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");
        await chefClient.PostAsJsonAsync("/api/chefs/me", NewChefProfile());

        var invalidRequest = new CreateFoodItemRequest
        {
            Name = "Zero Price Food",
            Description = "Invalid price test.",
            Price = -50.00m,
        };

        var response = await chefClient.PostAsJsonAsync("/api/chefs/me/foods", invalidRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("VALIDATION_ERROR", error!.Error.Code);
    }
}
