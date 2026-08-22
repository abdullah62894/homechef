using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;

namespace HomeChef.Api.Tests;

public class ReportEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public ReportEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Report",
            lastName = "Tester",
            email = $"rpt-{Guid.NewGuid():N}@test.com",
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
            displayName = $"Rpt Kitchen {Guid.NewGuid():N}",
            bio = "Reportable kitchen.",
            city = "Multan",
            area = "Cantt",
            cuisines = new[] { "Pakistani" },
        });
        Assert.Equal(HttpStatusCode.Created, createProfile.StatusCode);

        using var doc = JsonDocument.Parse(await createProfile.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId);
    }

    private static async Task<Guid> CreateFoodAsync(HttpClient chefClient)
    {
        var response = await chefClient.PostAsJsonAsync("/api/chefs/me/foods", new
        {
            name = $"Korma {Guid.NewGuid():N}",
            description = "Creamy korma.",
            price = 550,
            isAvailable = true,
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);
    }

    private static HttpContent ReportBody(string targetType, Guid targetId, string reason = "Spam", string? details = null) =>
        JsonContent.Create(new { targetType, targetId, reason, details });

    [Fact]
    public async Task Report_ChefFoodAndReview_AllCreatedAndVisibleToAdmin()
    {
        var admin = await LoginAsAdminAsync();
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");
        var foodId = await CreateFoodAsync(chefClient);

        var reviewResponse = await customer.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new
        {
            rating = 2,
            comment = "Slow delivery.",
        });
        Assert.Equal(HttpStatusCode.Created, reviewResponse.StatusCode);
        using var reviewDoc = JsonDocument.Parse(await reviewResponse.Content.ReadAsStringAsync());
        var reviewId = Guid.Parse(reviewDoc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        foreach (var (targetType, targetId) in new[]
                 {
                     ("ChefProfile", chefProfileId),
                     ("FoodItem", foodId),
                     ("Review", reviewId),
                 })
        {
            var response = await customer.PostAsync("/api/reports", ReportBody(targetType, targetId, details: "Looks off."));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var list = await admin.GetFromJsonAsync<ApiResponse<IReadOnlyList<ReportTestDto>>>("/api/admin/reports?status=Open");
        var mine = list!.Data.Where(r => r.TargetId == chefProfileId || r.TargetId == foodId || r.TargetId == reviewId).ToList();
        Assert.Equal(3, mine.Count);

        // Admin resolves the first and dismisses the second.
        var resolve = await admin.PostAsync($"/api/admin/reports/{mine[0].Id}/resolve", null);
        Assert.Equal(HttpStatusCode.OK, resolve.StatusCode);
        var dismiss = await admin.PostAsync($"/api/admin/reports/{mine[1].Id}/dismiss", null);
        Assert.Equal(HttpStatusCode.OK, dismiss.StatusCode);

        var openAfter = await admin.GetFromJsonAsync<ApiResponse<IReadOnlyList<ReportTestDto>>>("/api/admin/reports?status=Open");
        var stillOpen = openAfter!.Data.Count(r => r.TargetId == chefProfileId || r.TargetId == foodId || r.TargetId == reviewId);
        Assert.Equal(1, stillOpen);
    }

    [Fact]
    public async Task Report_DuplicateOpenReport_Rejected()
    {
        var (_, chefProfileId) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");

        var first = await customer.PostAsync("/api/reports", ReportBody("ChefProfile", chefProfileId));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var duplicate = await customer.PostAsync("/api/reports", ReportBody("ChefProfile", chefProfileId));
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        Assert.Contains("REPORT_DUPLICATE", await duplicate.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Report_RateLimitedAfterDailyQuota()
    {
        var customer = await RegisterAndGetClientAsync("Customer");

        // Default quota is 5 reports/day; distinct kitchens keep each report unique.
        for (var i = 0; i < 5; i++)
        {
            var (_, chefProfileId) = await CreateChefWithProfileAsync();
            var response = await customer.PostAsync("/api/reports", ReportBody("ChefProfile", chefProfileId));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var (_, extraChefId) = await CreateChefWithProfileAsync();
        var limited = await customer.PostAsync("/api/reports", ReportBody("ChefProfile", extraChefId));
        Assert.Equal(HttpStatusCode.BadRequest, limited.StatusCode);
        Assert.Contains("REPORT_RATE_LIMITED", await limited.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Report_UnknownTarget_Returns404()
    {
        var customer = await RegisterAndGetClientAsync("Customer");

        var response = await customer.PostAsync("/api/reports", ReportBody("FoodItem", Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("REPORT_TARGET_INVALID", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Report_RequiresAuthentication_AndAdminListRequiresAdmin()
    {
        var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.PostAsync("/api/reports", ReportBody("ChefProfile", Guid.NewGuid()))).StatusCode);

        var customer = await RegisterAndGetClientAsync("Customer");
        Assert.Equal(HttpStatusCode.Forbidden, (await customer.GetAsync("/api/admin/reports")).StatusCode);
    }

    [Fact]
    public async Task AbusePrevention_BlockedWordRejectedInMessageAndReview()
    {
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");

        var message = await customer.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId,
            body = $"This contains BADWORDTEST and should be rejected {Guid.NewGuid():N}.",
        });
        Assert.Equal(HttpStatusCode.BadRequest, message.StatusCode);
        Assert.Contains("CONTENT_BLOCKED", await message.Content.ReadAsStringAsync());

        var review = await customer.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new
        {
            rating = 1,
            comment = "Absolutely badwordtest service.",
        });
        Assert.Equal(HttpStatusCode.BadRequest, review.StatusCode);
        Assert.Contains("CONTENT_BLOCKED", await review.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AbusePrevention_MessageDailyLimit()
    {
        var (_, firstChefId) = await CreateChefWithProfileAsync();
        var (_, secondChefId) = await CreateChefWithProfileAsync();
        var customer = await RegisterAndGetClientAsync("Customer");

        // Default limit is 20 messages/day.
        for (var i = 0; i < 20; i++)
        {
            var target = i % 2 == 0 ? firstChefId : secondChefId;
            var response = await customer.PostAsJsonAsync("/api/messages", new
            {
                chefProfileId = target,
                body = $"Hello chef, question {i} {Guid.NewGuid():N}",
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        var limited = await customer.PostAsJsonAsync("/api/messages", new
        {
            chefProfileId = firstChefId,
            body = "One message too many.",
        });
        Assert.Equal(HttpStatusCode.BadRequest, limited.StatusCode);
        Assert.Contains("MESSAGE_RATE_LIMITED", await limited.Content.ReadAsStringAsync());
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

    private sealed class ReportTestDto
    {
        public Guid Id { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public Guid TargetId { get; set; }
        public string TargetLabel { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
