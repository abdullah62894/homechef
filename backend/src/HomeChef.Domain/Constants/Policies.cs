namespace HomeChef.Domain.Constants;

/// <summary>Named authorization policies.</summary>
public static class Policies
{
    public const string RequireCustomer = "RequireCustomer";
    public const string RequireChef = "RequireChef";
    public const string RequireAdmin = "RequireAdmin";
}