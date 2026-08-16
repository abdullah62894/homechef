namespace HomeChef.Application.Features.Reviews.Contracts;

public sealed class ChefRatingSummaryDto
{
    public Guid ChefProfileId { get; set; }

    public double AverageRating { get; set; }

    public int TotalReviews { get; set; }

    /// <summary>Star counts for ratings 1 through 5.</summary>
    public Dictionary<int, int> RatingDistribution { get; set; } = new()
    {
        [1] = 0,
        [2] = 0,
        [3] = 0,
        [4] = 0,
        [5] = 0,
    };
}
