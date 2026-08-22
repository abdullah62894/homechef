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
    public const string ChefProfileRequired = "CHEF_PROFILE_REQUIRED";
    public const string FoodItemNotFound = "FOOD_ITEM_NOT_FOUND";
    public const string FoodCategoryNotFound = "FOOD_CATEGORY_NOT_FOUND";
    public const string FoodItemForbidden = "FOOD_ITEM_FORBIDDEN";
    public const string ReviewNotFound = "REVIEW_NOT_FOUND";
    public const string SelfReviewForbidden = "SELF_REVIEW_FORBIDDEN";
    public const string DuplicateReview = "DUPLICATE_REVIEW";
    public const string ReviewForbidden = "REVIEW_FORBIDDEN";
    public const string MessageNotFound = "MESSAGE_NOT_FOUND";
    public const string MessageForbidden = "MESSAGE_FORBIDDEN";
    public const string SelfMessageForbidden = "SELF_MESSAGE_FORBIDDEN";
    public const string ChefProfileMissing = "CHEF_PROFILE_MISSING";
}