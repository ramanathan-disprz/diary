using System.Net;

namespace backend.Exceptions;

public class UnauthorizedAccessException : DisprzException
{
    public UnauthorizedAccessException()
        : base("Unauthorized access", HttpStatusCode.Forbidden)
    {
    }

    public UnauthorizedAccessException(string message)
        : base(message, HttpStatusCode.Forbidden)
    {
    }

    public UnauthorizedAccessException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Forbidden)
    {
    }
}