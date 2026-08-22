namespace HomeChef.Application.Features.Admin.Contracts;

/// <summary>Filters for the admin user listing.</summary>
public sealed class AdminUserQuery
{
    /// <summary>Case-insensitive match on email or name.</summary>
    public string? Search { get; init; }

    /// <summary>Restrict to users holding this role (e.g. "Chef").</summary>
    public string? Role { get; init; }
}
