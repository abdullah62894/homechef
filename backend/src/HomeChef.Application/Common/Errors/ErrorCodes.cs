namespace HomeChef.Application.Common.Errors;

/// <summary>
/// Stable, machine-readable error codes exposed through the API error contract.
/// </summary>
public static class ErrorCodes
{
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string LockedOut = "LOCKED_OUT";
    public const string EmailTaken = "EMAIL_TAKEN";
    public const string InvalidRole = "INVALID_ROLE";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string RegistrationFailed = "REGISTRATION_FAILED";
    public const string ChefProfileNotFound = "CHEF_PROFILE_NOT_FOUND";
    public const string ChefProfileExists = "CHEF_PROFILE_EXISTS";
}