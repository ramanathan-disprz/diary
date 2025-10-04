using System.Net;

namespace backend.Exceptions;

public class MalformedJwtException : DisprzException
{
    public MalformedJwtException()
        : base("Malformed JWT token", HttpStatusCode.Unauthorized)
    {
    }

    public MalformedJwtException(string message)
        : base(message, HttpStatusCode.Unauthorized)
    {
    }

    public MalformedJwtException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Unauthorized)
    {
    }
}