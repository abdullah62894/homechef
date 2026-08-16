using HomeChef.Application.Features.Auth.Contracts;

namespace HomeChef.Application.Features.Auth;

public interface IAuthService
{
    /// <summary>Registers a new user (Customer or Chef) and returns a token.</summary>
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>Validates credentials and returns a token on success.</summary>
    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Returns the profile of the authenticated user.</summary>
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}