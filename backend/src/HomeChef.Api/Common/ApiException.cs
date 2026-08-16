namespace HomeChef.Api.Common;

/// <summary>
/// An exception that maps to a specific HTTP status code and stable error code
/// in the API error contract.
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public string Code { get; }

    public ApiException(string code, string message, int statusCode = StatusCodes.Status400BadRequest)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}