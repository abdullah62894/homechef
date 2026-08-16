using Microsoft.AspNetCore.Identity;

namespace HomeChef.Domain.Identity;

/// <summary>
/// Application user. Uses Guid keys (UUIDs for public-facing identifiers).
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}