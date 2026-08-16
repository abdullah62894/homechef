using System.Net;
using System.Text.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;

namespace HomeChef.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into the consistent API error contract
/// { "error": { "code": "...", "message": "..." } }. Internal details are never
/// leaked to clients; they are written to structured logs instead.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            var statusCode = MapStatusCode(ex.Code);
            await WriteErrorAsync(context, statusCode, ex.Code, ex.Message);
        }
        catch (ApiException ex)
        {
            await WriteErrorAsync(context, ex.StatusCode, ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception during request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR", "An unexpected error occurred.");
        }
    }

    private static int MapStatusCode(string code) => code switch
    {
        ErrorCodes.InvalidCredentials or ErrorCodes.LockedOut => StatusCodes.Status401Unauthorized,
        ErrorCodes.FoodItemForbidden or
            ErrorCodes.SelfReviewForbidden or
            ErrorCodes.ReviewForbidden => StatusCodes.Status403Forbidden,
        ErrorCodes.EmailTaken or
            ErrorCodes.ChefProfileExists or
            ErrorCodes.DuplicateReview => StatusCodes.Status409Conflict,
        ErrorCodes.UserNotFound or
            ErrorCodes.ChefProfileNotFound or
            ErrorCodes.FoodItemNotFound or
            ErrorCodes.FoodCategoryNotFound or
            ErrorCodes.ReviewNotFound => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status400BadRequest,
    };

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(new ApiError(code, message));

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}