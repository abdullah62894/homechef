using HomeChef.Application.Security;

namespace HomeChef.Api.Common;

/// <summary>Delivers the JWT to the browser as an httpOnly cookie.</summary>
public static class AuthCookie
{
    public static void Set(HttpContext context, JwtOptions options, string token)
    {
        context.Response.Cookies.Append(
            options.CookieName,
            token,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = options.RequireSecureCookie,
                // Cross-origin (e.g. Vercel frontend + hosted API) requires
                // SameSite=None over HTTPS. Over plain HTTP (development)
                // browsers reject None, so fall back to Lax.
                SameSite = options.RequireSecureCookie ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddMinutes(options.ExpiresInMinutes),
            });
    }

    public static void Clear(HttpContext context, JwtOptions options)
    {
        context.Response.Cookies.Delete(
            options.CookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = options.RequireSecureCookie,
                SameSite = options.RequireSecureCookie ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/",
            });
    }
}