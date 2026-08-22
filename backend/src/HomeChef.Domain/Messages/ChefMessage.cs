using HomeChef.Domain.Chefs;
using HomeChef.Domain.Identity;

namespace HomeChef.Domain.Messages;

/// <summary>
/// A message a user sends to a chef through the contact feature.
/// </summary>
public class ChefMessage
{
    public Guid Id { get; set; }

    public Guid ChefProfileId { get; set; }

    public ChefProfile? ChefProfile { get; set; }

    public Guid SenderUserId { get; set; }

    public ApplicationUser? Sender { get; set; }

    public string Body { get; set; } = string.Empty;

    /// <summary>UTC timestamp of when the chef first read the message.</summary>
    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
