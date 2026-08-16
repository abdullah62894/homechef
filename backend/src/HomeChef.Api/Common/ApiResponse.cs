namespace HomeChef.Api.Common;

/// <summary>
/// Consistent success envelope for API responses.
/// Example: { "data": { ... }, "meta": { "page": 1 } }
/// </summary>
public sealed record ApiResponse<T>(T Data, object? Meta = null);

/// <summary>
/// Consistent error envelope for API responses.
/// Example: { "error": { "code": "CHEF_NOT_FOUND", "message": "..." } }
/// </summary>
public sealed record ApiError(string Code, string Message);

public sealed record ApiErrorResponse(ApiError Error);