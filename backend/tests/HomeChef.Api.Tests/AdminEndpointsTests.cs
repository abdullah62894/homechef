using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;

namespace HomeChef.Api.Tests;

public class AdminEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private const string AdminEmail = "admin@homechef.test";
    private const string AdminPassword = "Admin123!";

    private readonly HomeChefApiFactory _factory;

    public AdminEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> RegisterAndGetClientAsync(string role, string? email = null, string? password = null)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Admin",
            lastName = "Tester",
            email = email ?? $"adm-{Guid.NewGuid():N}@test.com",
            password = password ?? "Password123",
            role,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private async Task<HttpClient> LoginAsAdminAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AdminEmail,
            password = AdminPassword,
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return client;
    }

    private async Task<(HttpClient ChefClient, Guid ChefProfileId, string Email, string Password)> CreateChefWithProfileAsync()
    {
        var email = $"adm-chef-{Guid.NewGuid():N}@test.com";
        var chefClient = await RegisterAndGetClientAsync("Chef", email);
        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = $"Mod Kitchen {Guid.NewGuid():N}",
            bio = "Awaiting moderation.",
            city = "Islamabad",
            area = "F-7",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        using var doc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId, email, "Password123");
    }

    private static async Task<Guid> CreateFoodAsync(HttpClient chefClient)
    {
        var response = await chefClient.PostAsJsonAsync("/api/chefs/me/foods", new
        {
            name = $"Karahi {Guid.NewGuid():N}",
            description = "Spicy karahi.",
            price = 900,
            isAvailable = true,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);
    }

    [Fact]
    public async Task AdminEndpoints_RequireAdminRole()
    {
        var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/admin/users")).StatusCode);

        var customer = await RegisterAndGetClientAsync("Customer");
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/admin/users")).StatusCode);
    }

    [Fact]
    public async Task SeededAdmin_CanSuspendAndRestoreUser()
    {
        var admin = await LoginAsAdminAsync();
        var (chefClient, _, email, password) = await CreateChefWithProfileAsync();

        // 1. Admin finds the chef account via search
        var list = await admin.GetFromJsonAsync<ApiResponse<IReadOnlyList<AdminTestUserDto>>>("/api/admin/users?search=" + Uri.EscapeDataString(email));
        var target = Assert.Single(list!.Data);
        Assert.Contains("Chef", target.Roles);
        Assert.False(target.IsSuspended);

        // 2. Suspension blocks the account's login
        var suspend = await admin.PostAsync($"/api/admin/users/{target.Id}/suspend", null);
        Assert.Equal(HttpStatusCode.OK, suspend.StatusCode);

        var loginClient = _factory.CreateClient();
        var blockedLogin = await loginClient.PostAsJsonAsync("/api/auth/login", new { email, password = password });
        Assert.Equal(HttpStatusCode.Unauthorized, blockedLogin.StatusCode);
        Assert.Contains("LOCKED_OUT", await blockedLogin.Content.ReadAsStringAsync());

        // 3. Restore unblocks it
        var restore = await admin.PostAsync($"/api/admin/users/{target.Id}/restore", null);
        Assert.Equal(HttpStatusCode.OK, restore.StatusCode);

        var okLogin = await loginClient.PostAsJsonAsync("/api/auth/login", new { email, password = password });
        Assert.Equal(HttpStatusCode.OK, okLogin.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotSuspendSelf()
    {
        var admin = await LoginAsAdminAsync();

        var me = await admin.GetFromJsonAsync<ApiResponse<AdminTestUserDto>>("/api/users/me");
        var response = await admin.PostAsync($"/api/admin/users/{me!.Data.Id}/suspend", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("ADMIN_SELF_SUSPEND_FORBIDDEN", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Admin_CanModerateReviews()
    {
        var admin = await LoginAsAdminAsync();
        var (_, chefProfileId, _, _) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");

        var reviewResponse = await customer.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new
        {
            rating = 1,
            comment = "Abusive text that moderation removes.",
        });
        Assert.Equal(HttpStatusCode.Created, reviewResponse.StatusCode);

        using var reviewDoc = JsonDocument.Parse(await reviewResponse.Content.ReadAsStringAsync());
        var reviewId = Guid.Parse(reviewDoc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        // 1. Review appears in the moderation list
        var reviews = await admin.GetFromJsonAsync<ApiResponse<IReadOnlyList<AdminTestReviewDto>>>("/api/admin/reviews");
        Assert.Contains(reviews!.Data, r => r.Id == reviewId);

        // 2. Admin removes it
        var delete = await admin.DeleteAsync($"/api/admin/reviews/{reviewId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var reviewsAfter = await admin.GetFromJsonAsync<ApiResponse<IReadOnlyList<AdminTestReviewDto>>>("/api/admin/reviews");
        Assert.DoesNotContain(reviewsAfter!.Data, r => r.Id == reviewId);

        // 3. It is gone from the public chef reviews and the summary resets
        var publicReviews = await _factory.CreateClient()
            .GetFromJsonAsync<ApiResponse<IReadOnlyList<object>>>($"/api/chefs/{chefProfileId}/reviews");
        Assert.Empty(publicReviews!.Data);

        var summary = await _factory.CreateClient()
            .GetFromJsonAsync<ApiResponse<AdminTestSummaryDto>>($"/api/chefs/{chefProfileId}/reviews/summary");
        Assert.Equal(0, summary!.Data.TotalReviews);
    }

    [Fact]
    public async Task Admin_CanDeleteFoodItem()
    {
        var admin = await LoginAsAdminAsync();
        var (chefClient, _, _, _) = await CreateChefWithProfileAsync();
        var foodId = await CreateFoodAsync(chefClient);

        var delete = await admin.DeleteAsync($"/api/admin/foods/{foodId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var gone = await _factory.CreateClient().GetAsync($"/api/foods/{foodId}");
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task Admin_CanRemoveKitchen_KeepingAccount()
    {
        var admin = await LoginAsAdminAsync();
        var (chefClient, chefProfileId, email, password) = await CreateChefWithProfileAsync();
        var foodId = await CreateFoodAsync(chefClient);

        var delete = await admin.DeleteAsync($"/api/admin/chefs/{chefProfileId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        // Public kitchen and its dishes are gone
        var profileGone = await _factory.CreateClient().GetAsync($"/api/chefs/{chefProfileId}");
        Assert.Equal(HttpStatusCode.NotFound, profileGone.StatusCode);
        var foodGone = await _factory.CreateClient().GetAsync($"/api/foods/{foodId}");
        Assert.Equal(HttpStatusCode.NotFound, foodGone.StatusCode);

        // The user account still exists and can sign in
        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    private sealed class AdminTestUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
        public bool IsSuspended { get; set; }
        public Guid? ChefProfileId { get; set; }
    }

    private sealed class AdminTestReviewDto
    {
        public Guid Id { get; set; }
        public Guid ChefProfileId { get; set; }
        public string ChefDisplayName { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    private sealed class AdminTestSummaryDto
    {
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
    }
}
