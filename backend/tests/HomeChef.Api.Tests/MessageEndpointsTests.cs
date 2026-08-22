using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Messages.Contracts;

namespace HomeChef.Api.Tests;

public class MessageEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public MessageEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private sealed record RegisterRequestPayload(string Email, string Role)
    {
        public object ToJson() => new
        {
            firstName = "Message",
            lastName = "Tester",
            email = Email,
            password = "Password123",
            role = Role,
        };
    }

    private async Task<HttpClient> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var email = $"msg-{Guid.NewGuid():N}@test.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequestPayload(email, role).ToJson());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return client;
    }

    private async Task<(HttpClient ChefClient, Guid ChefProfileId)> CreateChefWithProfileAsync()
    {
        var chefClient = await RegisterAndGetClientAsync("Chef");
        var createProfile = await chefClient.PostAsJsonAsync("/api/chefs/me", new
        {
            displayName = $"Msg Kitchen {Guid.NewGuid():N}",
            bio = "Delicious meals for contact tests.",
            city = "Rawalpindi",
            area = "Saddar",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        using var doc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId);
    }

    private async Task<Guid> SendSampleMessageAsync(HttpClient sender, Guid chefProfileId)
    {
        var response = await sender.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId,
            body = $"Hello chef! Is the karahi spicy? {Guid.NewGuid():N}",
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);
    }

    [Fact]
    public async Task ContactChef_SendInboxMarkRead_Lifecycle()
    {
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();
        var customerClient = await RegisterAndGetClientAsync("Customer");

        // 1. Customer sends a message to the chef
        var messageId = await SendSampleMessageAsync(customerClient, chefProfileId);

        // 2. Chef sees it in inbox with unread badge
        var unreadBefore = await chefClient.GetFromJsonAsync<ApiResponse<int>>("/api/messages/unread-count");
        Assert.Equal(1, unreadBefore!.Data);

        // 3. Inbox lists it with sender name
        var inboxResponse = await chefClient.GetAsync("/api/messages/inbox");
        Assert.Equal(HttpStatusCode.OK, inboxResponse.StatusCode);
        var inbox = await inboxResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ChefMessageDto>>>();
        var message = Assert.Single(inbox!.Data);
        Assert.Equal(messageId, message.Id);
        Assert.False(string.IsNullOrWhiteSpace(message.SenderName));
        Assert.False(string.IsNullOrWhiteSpace(message.ChefDisplayName));
        Assert.Null(message.ReadAtUtc);

        // 4. Chef marks it read
        var readResponse = await chefClient.PostAsync($"/api/messages/{messageId}/read", null);
        Assert.Equal(HttpStatusCode.NoContent, readResponse.StatusCode);

        // 5. Unread count drops to zero; message shows read timestamp
        var unreadAfter = await chefClient.GetFromJsonAsync<ApiResponse<int>>("/api/messages/unread-count");
        Assert.Equal(0, unreadAfter!.Data);

        var sentResponse = await customerClient.GetAsync("/api/messages/sent");
        Assert.Equal(HttpStatusCode.OK, sentResponse.StatusCode);
        var sent = await sentResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ChefMessageDto>>>();
        var sentMessage = Assert.Single(sent!.Data);
        Assert.Equal(messageId, sentMessage.Id);
        Assert.NotNull(sentMessage.ReadAtUtc);
    }

    [Fact]
    public async Task ContactChef_SendingToOwnKitchen_Forbidden()
    {
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();

        var response = await chefClient.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId,
            body = "Note to self.",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("SELF_MESSAGE_FORBIDDEN", error!.Error.Code);
    }

    [Fact]
    public async Task ContactChef_UnknownChef_ReturnsNotFound()
    {
        var customerClient = await RegisterAndGetClientAsync("Customer");

        var response = await customerClient.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId = Guid.NewGuid(),
            body = "Anyone there?",
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("CHEF_PROFILE_NOT_FOUND", error!.Error.Code);
    }

    [Fact]
    public async Task ContactChef_InboxRequiresChefRole()
    {
        var (_, chefProfileId) = await CreateChefWithProfileAsync();
        var customerClient = await RegisterAndGetClientAsync("Customer");
        await SendSampleMessageAsync(customerClient, chefProfileId);

        var inboxResponse = await customerClient.GetAsync("/api/messages/inbox");
        Assert.Equal(HttpStatusCode.Forbidden, inboxResponse.StatusCode);

        var readResponse = await customerClient.PostAsync($"/api/messages/{Guid.NewGuid()}/read", null);
        Assert.Equal(HttpStatusCode.Forbidden, readResponse.StatusCode);
    }

    [Fact]
    public async Task ContactChef_MarkReadByOtherChef_Forbidden()
    {
        var (ownerClient, chefProfileId) = await CreateChefWithProfileAsync();
        var (otherChefClient, _) = await CreateChefWithProfileAsync();
        var customerClient = await RegisterAndGetClientAsync("Customer");

        var messageId = await SendSampleMessageAsync(customerClient, chefProfileId);

        // Another chef cannot mark someone else's message as read.
        var foreignResponse = await otherChefClient.PostAsync($"/api/messages/{messageId}/read", null);
        Assert.Equal(HttpStatusCode.Forbidden, foreignResponse.StatusCode);

        // The owner can still read it afterwards.
        var ownerResponse = await ownerClient.PostAsync($"/api/messages/{messageId}/read", null);
        Assert.Equal(HttpStatusCode.NoContent, ownerResponse.StatusCode);
    }

    [Fact]
    public async Task Messages_Unauthenticated_ReturnsUnauthorized()
    {
        var anonClient = _factory.CreateClient();

        var sendResponse = await anonClient.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId = Guid.NewGuid(),
            body = "Hello?",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, sendResponse.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await anonClient.GetAsync("/api/messages/inbox")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonClient.GetAsync("/api/messages/sent")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonClient.GetAsync("/api/messages/unread-count")).StatusCode);
    }
}
