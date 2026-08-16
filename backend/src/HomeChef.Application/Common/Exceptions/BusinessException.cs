namespace HomeChef.Application.Common.Exceptions;

/// <summary>
/// A business-rule failure carrying a stable error code. The API layer maps
/// the code to an HTTP status and serializes it into the error contract.
/// </summary>
public class BusinessException : Exception
{
    public string Code { get; }

    public BusinessException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}