namespace HomeChef.Application.Features.Admin.Contracts;

public sealed class AdminUserDto
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string[] Roles { get; init; }

    /// <summary>True while the account is suspended (Identity lockout).</summary>
    public required bool IsSuspended { get; init; }

    /// <summary>Id of the user's chef profile, when they run a kitchen.</summary>
    public Guid? ChefProfileId { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}

public sealed class AdminReviewDto
{
    public required Guid Id { get; init; }

    public required Guid ChefProfileId { get; init; }

    public required string ChefDisplayName { get; init; }

    public required Guid CustomerUserId { get; init; }

    public required string ReviewerName { get; init; }

    public required int Rating { get; init; }

    public required string Comment { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
}
