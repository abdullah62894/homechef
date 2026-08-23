using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;

namespace HomeChef.Api.Tests;

/// <summary>
/// Stage 9 additions: self-service profile/password management and full
/// admin CRUD over user accounts.
/// </summary>
public class UserManagementTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public UserManagementTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> RegisterAndGetClientAsync(string role, string? email = null)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Manage",
            lastName = "Tester",
            email = email ?? $"usr-{Guid.NewGuid():N}@test.com",
            password = "Password123",
            role,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private static async Task<(HttpClient Client, Guid UserId)> RegisterAndCaptureUserAsync(
        Func<Task<HttpClient>> register, string email)
    {
        var client = await register();
        return (client, await GetUserIdAsync(client));
    }

    private static async Task<Guid> GetUserIdAsync(HttpClient client)
    {
        var me = await client.GetFromJsonAsync<ApiResponse<UserTestDto>>("/api/users/me");
        return me!.Data.Id;
    }

    private async Task<HttpClient> LoginAsAdminAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@homechef.test",
            password = "Admin123!",
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return client;
    }

    [Fact]
    public async Task SelfService_UpdateProfileAndChangePassword()
    {
        var email = $"self-{Guid.NewGuid():N}@test.com";
        var client = await RegisterAndGetClientAsync("Customer", email);

        // 1. Update own profile names.
        var update = await client.PutAsJsonAsync("/api/users/me", new
        {
            firstName = "Updated",
            lastName = "Name",
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ApiResponse<UserTestDto>>();
        Assert.Equal("Updated", updated!.Data.FirstName);
        Assert.Equal("Name", updated.Data.LastName);

        // 2. Wrong current password is rejected.
        var wrong = await client.PutAsJsonAsync("/api/users/me/password", new
        {
            currentPassword = "NotMyPassword1",
            newPassword = "NewPassword456",
        });
        Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
        Assert.Contains("WRONG_CURRENT_PASSWORD", await wrong.Content.ReadAsStringAsync());

        // 3. Change password, then log in with the new one.
        var change = await client.PutAsJsonAsync("/api/users/me/password", new
        {
            currentPassword = "Password123",
            newPassword = "NewPassword456",
        });
        Assert.Equal(HttpStatusCode.NoContent, change.StatusCode);

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "NewPassword456",
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Admin_CreateUser_ThenUserCanLogIn()
    {
        var admin = await LoginAsAdminAsync();
        var email = $"created-{Guid.NewGuid():N}@test.com";

        var create = await admin.PostAsJsonAsync("/api/admin/users", new
        {
            email,
            password = "CreatedPass123",
            firstName = "Created",
            lastName = "ByAdmin",
            role = "Chef",
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ApiResponse<AdminUserTestDto>>();
        Assert.Contains("Chef", created!.Data.Roles);
        Assert.Null(created.Data.ChefProfileId);

        // Duplicate email conflicts.
        var duplicate = await admin.PostAsJsonAsync("/api/admin/users", new
        {
            email,
            password = "CreatedPass123",
            firstName = "Dup",
            lastName = "User",
            role = "Customer",
        });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        // The created account can sign in with the admin-chosen password.
        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "CreatedPass123",
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
    }

    [Fact]
    public async Task Admin_EditUser_RoleAndNames()
    {
        var admin = await LoginAsAdminAsync();
        var email = $"edited-{Guid.NewGuid():N}@test.com";
        var client = await RegisterAndGetClientAsync("Customer", email);
        var userId = await GetUserIdAsync(client);

        var update = await admin.PutAsJsonAsync($"/api/admin/users/{userId}", new
        {
            firstName = "Renamed",
            lastName = "User",
            role = "Chef",
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<ApiResponse<AdminUserTestDto>>();
        Assert.Equal("Renamed", updated!.Data.FirstName);
        Assert.Contains("Chef", updated.Data.Roles);

        var me = await client.GetFromJsonAsync<ApiResponse<UserTestDto>>("/api/users/me");
        Assert.Equal("Renamed", me!.Data.FirstName);
    }

    [Fact]
    public async Task Admin_DemoteChef_RemovesKitchen()
    {
        var admin = await LoginAsAdminAsync();
        var chefClient = await RegisterAndGetClientAsync("Chef");
        var userId = await GetUserIdAsync(chefClient);

        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = $"Demote Kitchen {Guid.NewGuid():N}",
            bio = "Will be removed on demotion.",
            city = "Karachi",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);
        using var doc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        var demote = await admin.PutAsJsonAsync($"/api/admin/users/{userId}", new
        {
            firstName = "Ex",
            lastName = "Chef",
            role = "Customer",
        });
        Assert.Equal(HttpStatusCode.OK, demote.StatusCode);

        var profileGone = await _factory.CreateClient().GetAsync($"/api/chefs/{chefProfileId}");
        Assert.Equal(HttpStatusCode.NotFound, profileGone.StatusCode);
    }

    [Fact]
    public async Task Admin_CannotChangeOwnRole_OrDeleteSelf()
    {
        var admin = await LoginAsAdminAsync();
        var adminId = await GetUserIdAsync(admin);

        var roleChange = await admin.PutAsJsonAsync($"/api/admin/users/{adminId}", new
        {
            firstName = "Admin",
            lastName = "User",
            role = "Customer",
        });
        Assert.Equal(HttpStatusCode.BadRequest, roleChange.StatusCode);
        Assert.Contains("ADMIN_SELF_ROLE_FORBIDDEN", await roleChange.Content.ReadAsStringAsync());

        var delete = await admin.DeleteAsync($"/api/admin/users/{adminId}");
        Assert.Equal(HttpStatusCode.BadRequest, delete.StatusCode);
        Assert.Contains("ADMIN_SELF_DELETE_FORBIDDEN", await delete.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Admin_SetPassword_OverridesOldOne()
    {
        var admin = await LoginAsAdminAsync();
        var email = $"reset-{Guid.NewGuid():N}@test.com";
        await RegisterAndGetClientAsync("Customer", email);
        var client = _factory.CreateClient();
        var me = await client.PostAsJsonAsync("/api/auth/login", new { email, password = "Password123" });
        var userId = Guid.Parse((await me.Content.ReadFromJsonAsync<ApiResponse<UserTestDto>>())!.Data.Id.ToString());

        var set = await admin.PutAsJsonAsync($"/api/admin/users/{userId}/password", new
        {
            newPassword = "AdminSet123",
        });
        Assert.Equal(HttpStatusCode.OK, set.StatusCode);

        var oldLogin = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password = "Password123" });
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password = "AdminSet123" });
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task Admin_DeleteUser_AccountAndKitchenGone()
    {
        var admin = await LoginAsAdminAsync();
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();
        var userId = await GetUserIdAsync(chefClient);

        var delete = await admin.DeleteAsync($"/api/admin/users/{userId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var profileGone = await _factory.CreateClient().GetAsync($"/api/chefs/{chefProfileId}");
        Assert.Equal(HttpStatusCode.NotFound, profileGone.StatusCode);

        var login = await _factory.CreateClient().PostAsJsonAsync("/api/auth/login", new
        {
            email = deletedChefEmail!,
            password = "Password123",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    private string? deletedChefEmail;

    private async Task<(HttpClient ChefClient, Guid ChefProfileId)> CreateChefWithProfileAsync()
    {
        deletedChefEmail = $"del-{Guid.NewGuid():N}@test.com";
        var chefClient = await RegisterAndGetClientAsync("Chef", deletedChefEmail);
        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = $"Doomed Kitchen {Guid.NewGuid():N}",
            bio = "Deleted with its owner.",
            city = "Lahore",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        using var doc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId);
    }

    private sealed class UserTestDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
    }

    private sealed class AdminUserTestDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = [];
        public Guid? ChefProfileId { get; set; }
    }
}
