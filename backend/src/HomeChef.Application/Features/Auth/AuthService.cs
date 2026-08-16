using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Auth.Contracts;
using HomeChef.Application.Security;
using HomeChef.Domain.Constants;
using HomeChef.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace HomeChef.Application.Features.Auth;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var role = string.IsNullOrWhiteSpace(request.Role) ? Roles.Customer : request.Role.Trim();

        if (!Roles.SelfService.Contains(role))
        {
            throw new BusinessException(ErrorCodes.InvalidRole, $"Role '{role}' cannot be self-assigned.");
        }

        var email = request.Email.Trim();

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            throw new BusinessException(ErrorCodes.EmailTaken, "An account with this email already exists.");
        }

        var now = DateTime.UtcNow;

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new BusinessException(
                ErrorCodes.RegistrationFailed,
                string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            throw new BusinessException(
                ErrorCodes.RegistrationFailed,
                string.Join(" ", roleResult.Errors.Select(e => e.Description)));
        }

        var roles = new[] { role };
        var token = _tokenService.CreateAccessToken(user, roles);

        return new AuthResult { Token = token, User = ToDto(user, roles) };
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());

        if (user is null)
        {
            throw new BusinessException(ErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            throw new BusinessException(ErrorCodes.LockedOut, "Account is temporarily locked. Try again later.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new BusinessException(ErrorCodes.LockedOut, "Account is temporarily locked. Try again later.");
            }

            throw new BusinessException(ErrorCodes.InvalidCredentials, "Invalid email or password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateAccessToken(user, roles);

        return new AuthResult { Token = token, User = ToDto(user, roles) };
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            throw new BusinessException(ErrorCodes.UserNotFound, "User was not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return ToDto(user, roles);
    }

    private static UserDto ToDto(ApplicationUser user, IEnumerable<string> roles)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles.ToArray(),
            CreatedAtUtc = user.CreatedAtUtc,
        };
    }
}