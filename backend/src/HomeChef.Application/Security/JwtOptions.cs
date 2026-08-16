namespace HomeChef.Application.Security;

/// <summary>Bound from the "Jwt" configuration section.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Access token lifetime in minutes.</summary>
    public int ExpiresInMinutes { get; set; } = 60;

    /// <summary>Cookie name used to deliver the token to the browser.</summary>
    public string CookieName { get; set; } = "HomeChef.Auth";

    /// <summary>Marks the auth cookie Secure. False in development over HTTP.</summary>
    public bool RequireSecureCookie { get; set; } = true;
}