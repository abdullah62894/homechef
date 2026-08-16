using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Reviews.Contracts;

namespace HomeChef.Api.Tests;

public class ReviewEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public ReviewEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private static RegisterRequestPayload NewUser(string role) =>
        new($"rvw-{Guid.NewGuid():N}@test.com", role);

    private async Task<(HttpClient Client, Guid UserId)> RegisterAndGetClientAsync(string role)
    {
        var client = _factory.CreateClient();
        var payload = NewUser(role);
        var response = await client.PostAsJsonAsync("/api/auth/register", payload.ToJson());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var userId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (client, userId);
    }

    private sealed record RegisterRequestPayload(string Email, string Role)
    {
        public object ToJson() => new
        {
            firstName = "Reviewer",
            lastName = "Test",
            email = Email,
            password = "Password123",
            role = Role,
        };
    }

    private async Task<(HttpClient ChefClient, Guid ChefProfileId)> CreateChefWithProfileAsync()
    {
        var (chefClient, _) = await RegisterAndGetClientAsync("Chef");
        var create = await chefClient.PostAsJsonAsync("/api/chefs/me", new CreateChefProfileRequest
        {
            DisplayName = $"Kitchen {Guid.NewGuid():N}",
            Bio = "Delicious home cooking for review tests.",
            City = "Lahore",
            Area = "Gulberg",
            Cuisines = ["Pakistani", "Desserts"],
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        using var doc = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var chefProfileId = Guid.Parse(doc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        return (chefClient, chefProfileId);
    }

    [Fact]
    public async Task ReviewLifecycle_CreateReadUpdateDelete_WorksCorrectly()
    {
        var (_, chefProfileId) = await CreateChefWithProfileAsync();
        var (customerClient, _) = await RegisterAndGetClientAsync("Customer");

        // 1. Create Review
        var createResponse = await customerClient.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new CreateReviewRequest
        {
            Rating = 5,
            Comment = "Outstanding flavor and piping hot food!",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponse<ReviewDto>>();
        Assert.NotNull(created?.Data);
        Assert.Equal(5, created.Data.Rating);
        Assert.Equal("Outstanding flavor and piping hot food!", created.Data.Comment);

        var reviewId = created.Data.Id;

        // 2. Duplicate Review Submission returns Conflict (DUPLICATE_REVIEW)
        var dupResponse = await customerClient.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new CreateReviewRequest
        {
            Rating = 4,
            Comment = "Second review should fail",
        });
        Assert.Equal(HttpStatusCode.Conflict, dupResponse.StatusCode);
        var dupError = await dupResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("DUPLICATE_REVIEW", dupError!.Error.Code);

        // 3. Public summary shows 5.0 avg and 1 total
        var anonClient = _factory.CreateClient();
        var summaryResponse = await anonClient.GetAsync($"/api/chefs/{chefProfileId}/reviews/summary");
        Assert.Equal(HttpStatusCode.OK, summaryResponse.StatusCode);
        var summary = await summaryResponse.Content.ReadFromJsonAsync<ApiResponse<ChefRatingSummaryDto>>();
        Assert.Equal(1, summary!.Data.TotalReviews);
        Assert.Equal(5.0, summary.Data.AverageRating);
        Assert.Equal(1, summary.Data.RatingDistribution[5]);

        // 4. Update Review
        var updateResponse = await customerClient.PutAsJsonAsync($"/api/reviews/{reviewId}", new UpdateReviewRequest
        {
            Rating = 4,
            Comment = "Updated review: very good!",
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponse<ReviewDto>>();
        Assert.Equal(4, updated!.Data.Rating);
        Assert.Equal("Updated review: very good!", updated.Data.Comment);

        // 5. Another customer cannot edit/delete this review
        var (otherCustomer, _) = await RegisterAndGetClientAsync("Customer");
        var unauthorizedUpdate = await otherCustomer.PutAsJsonAsync($"/api/reviews/{reviewId}", new UpdateReviewRequest
        {
            Rating = 1,
            Comment = "Hacked comment",
        });
        Assert.Equal(HttpStatusCode.Forbidden, unauthorizedUpdate.StatusCode);

        var unauthorizedDelete = await otherCustomer.DeleteAsync($"/api/reviews/{reviewId}");
        Assert.Equal(HttpStatusCode.Forbidden, unauthorizedDelete.StatusCode);

        // 6. Delete Review by owner
        var deleteResponse = await customerClient.DeleteAsync($"/api/reviews/{reviewId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // 7. Verify reviews list is now empty
        var listResponse = await anonClient.GetAsync($"/api/chefs/{chefProfileId}/reviews");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listData = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<ReviewDto>>>();
        Assert.Empty(listData!.Data);
    }

    [Fact]
    public async Task CreateReview_ChefReviewingSelf_ReturnsForbidden()
    {
        var (chefClient, chefProfileId) = await CreateChefWithProfileAsync();

        var response = await chefClient.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new CreateReviewRequest
        {
            Rating = 5,
            Comment = "I love my own food!",
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("SELF_REVIEW_FORBIDDEN", error!.Error.Code);
    }

    [Fact]
    public async Task CreateReview_InvalidRating_ReturnsValidationError()
    {
        var (_, chefProfileId) = await CreateChefWithProfileAsync();
        var (customerClient, _) = await RegisterAndGetClientAsync("Customer");

        var response = await customerClient.PostAsJsonAsync($"/api/chefs/{chefProfileId}/reviews", new
        {
            rating = 6, // Exceeds 1-5
            comment = "Invalid rating value",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateReview_Unauthenticated_ReturnsUnauthorized()
    {
        var anonClient = _factory.CreateClient();

        var response = await anonClient.PostAsJsonAsync($"/api/chefs/{Guid.NewGuid()}/reviews", new CreateReviewRequest
        {
            Rating = 5,
            Comment = "Anonymous review",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
