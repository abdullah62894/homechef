namespace HomeChef.Application.Features.Reports;

/// <summary>
/// Abuse-prevention settings (bound from the "Moderation" section).
/// </summary>
public sealed class ModerationOptions
{
    public const string SectionName = "Moderation";

    /// <summary>
    /// Naive blocklist: a normalized substring match on user-written text
    /// (message bodies, review comments, report details).
    /// </summary>
    public string[] BlockedWords { get; set; } = [];

    /// <summary>Max reports a single user may submit per day.</summary>
    public int MaxReportsPerDay { get; set; } = 5;
}

/// <summary>
/// Message abuse-prevention settings (bound from the "Messages" section).
/// </summary>
public sealed class MessagingOptions
{
    public const string SectionName = "Messages";

    /// <summary>Max contact messages a user may send per day.</summary>
    public int MaxPerDay { get; set; } = 20;
}
