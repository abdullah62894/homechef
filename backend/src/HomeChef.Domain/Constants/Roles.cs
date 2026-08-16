namespace HomeChef.Domain.Constants;

/// <summary>Well-known roles used by authorization policies.</summary>
public static class Roles
{
    public const string Customer = "Customer";
    public const string Chef = "Chef";
    public const string Admin = "Admin";
    public const string Moderator = "Moderator";

    /// <summary>Roles a user may request at self-registration.</summary>
    public static readonly IReadOnlyList<string> SelfService = [Customer, Chef];
}