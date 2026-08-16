namespace HomeChef.Application.Features.Reviews.Contracts;

public sealed class ReviewDto
{
    public Guid Id { get; set; }

    public Guid ChefProfileId { get; set; }

    public Guid CustomerUserId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
