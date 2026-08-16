using HomeChef.Domain.Chefs;
using HomeChef.Domain.Identity;

namespace HomeChef.Domain.Reviews;

/// <summary>
/// A review and rating left by an authenticated customer for a home chef.
/// </summary>
public class Review
{
    public Guid Id { get; set; }

    public Guid ChefProfileId { get; set; }

    public ChefProfile? ChefProfile { get; set; }

    public Guid CustomerUserId { get; set; }

    public ApplicationUser? CustomerUser { get; set; }

    /// <summary>Star rating from 1 to 5.</summary>
    public int Rating { get; set; }

    /// <summary>Customer's written feedback.</summary>
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
