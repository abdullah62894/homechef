namespace HomeChef.Application.Features.Messages.Contracts;

public sealed class ChefMessageDto
{
    public Guid Id { get; set; }

    public Guid ChefProfileId { get; set; }

    public string ChefDisplayName { get; set; } = string.Empty;

    public Guid SenderUserId { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    /// <summary>UTC timestamp of when the chef first read the message, if any.</summary>
    public DateTime? ReadAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
