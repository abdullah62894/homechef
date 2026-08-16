using HomeChef.Domain.Identity;

namespace HomeChef.Application.Security;

public interface ITokenService
{
    /// <summary>Creates a signed JWT for the user with their roles.</summary>
    string CreateAccessToken(ApplicationUser user, IEnumerable<string> roles);
}