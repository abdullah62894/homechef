namespace HomeChef.Domain.Messages;

/// <summary>
/// Contact Us form submissions stored in the database for admin review.
/// </summary>
public class ContactMessage
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    /// <summary>Status: New, Read, Resolved, Archived.</summary>
    public string Status { get; set; } = "New";

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ReadAtUtc { get; set; }
}
