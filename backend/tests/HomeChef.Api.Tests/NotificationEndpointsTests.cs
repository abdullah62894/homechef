using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;

namespace HomeChef.Api.Tests;

public class NotificationEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public NotificationEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Notif",
            lastName = "Tester",
            email = $"ntf-{Guid.NewGuid():N}@test.com",
            password = "Password123",
            role,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private async Task<(HttpClient ChefClient, Guid ChefProfileId)> CreateChefWithProfileAsync()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");
        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = $"Notif Kitchen {Guid.NewGuid():N}",
            bio = "Notifiable kitchen.",
            city = "Peshawar",
            area = "University Town",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        using var doc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId);
    }

    [Fact]
    public async Task MessageAndReview_CreateChefNotifications()
    {
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");

        var message = await customer.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId,
            body = "Do you cater for 20 people?",
        });
        Assert.Equal(HttpStatusCode.Created, message.StatusCode);

        var review = await customer.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new
        {
            rating = 5,
            comment = "Fantastic food, highly recommended!",
        });
        Assert.Equal(HttpStatusCode.Created, review.StatusCode);

        var unread = await chefClient.GetFromJsonAsync<ApiResponse<int>>("/api/notifications/unread-count");
        Assert.Equal(2, unread!.Data);

        var list = await chefClient.GetFromJsonAsync<ApiResponse<IReadOnlyList<NotificationTestDto>>>("/api/notifications");
        Assert.Equal(2, list!.Data.Count);

        var reviewNotification = list.Data.FirstOrDefault(n => n.Type == "NewReview");
        Assert.NotNull(reviewNotification);
        Assert.Contains("5★", reviewNotification!.Body);
        Assert.Null(reviewNotification.ReadAtUtc);

        var messageNotification = list.Data.FirstOrDefault(n => n.Type == "NewMessage");
        Assert.NotNull(messageNotification);
        Assert.Equal("New message", messageNotification!.Title);
    }

    [Fact]
    public async Task MarkRead_And_ReadAll_ClearUnread()
    {
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");

        await customer.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId,
            body = "First message for read tests.",
        });
        await customer.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId,
            body = "Second message for read tests.",
        });

        var list = await chefClient.GetFromJsonAsync<ApiResponse<IReadOnlyList<NotificationTestDto>>>("/api/notifications");
        Assert.Equal(2, list!.Data.Count);

        // Mark a single notification read.
        var read = await chefClient.PostAsync($"/api/notifications/{list.Data[0].Id}/read", null);
        Assert.Equal(HttpStatusCode.NoContent, read.StatusCode);

        var unreadAfterOne = await chefClient.GetFromJsonAsync<ApiResponse<int>>("/api/notifications/unread-count");
        Assert.Equal(1, unreadAfterOne!.Data);

        // Read-all clears the rest.
        var readAll = await chefClient.PostAsync("/api/notifications/read-all", null);
        Assert.Equal(HttpStatusCode.NoContent, readAll.StatusCode);

        var unreadAfterAll = await chefClient.GetFromJsonAsync<ApiResponse<int>>("/api/notifications/unread-count");
        Assert.Equal(0, unreadAfterAll!.Data);
    }

    [Fact]
    public async Task Notifications_ArePrivateToTheirOwner()
    {
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();
        var (otherChef, _) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");

        await customer.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId,
            body = "Only the owning chef should see this notification.",
        });

        var list = await chefClient.GetFromJsonAsync<ApiResponse<IReadOnlyList<NotificationTestDto>>>("/api/notifications");
        var notificationId = Assert.Single(list!.Data).Id;

        // Another chef may not read it.
        var forbidden = await otherChef.PostAsync($"/api/notifications/{notificationId}/read", null);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        // And the other chef has no notifications of their own.
        var otherList = await otherChef.GetFromJsonAsync<ApiResponse<IReadOnlyList<NotificationTestDto>>>("/api/notifications");
        Assert.Empty(otherList!.Data);
    }

    [Fact]
    public async Task Notifications_RequireAuthentication()
    {
        var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/notifications")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/notifications/unread-count")).StatusCode);
    }

    private sealed class NotificationTestDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime? ReadAtUtc { get; set; }
    }
}
