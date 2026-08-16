namespace HomeChef.Application.Features.Auth.Contracts;

public sealed class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public IReadOnlyList<string> Roles { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }
}

public sealed class AuthResult
{
    public string Token { get; set; } = string.Empty;

    public UserDto User { get; set; } = new();
}