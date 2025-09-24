using System.Net;

namespace backend.Exceptions;

public class InvalidCredentialsException : DisprzException
{
    public InvalidCredentialsException()
        : base("Invalid credentials", HttpStatusCode.Unauthorized)
    {
    }

    public InvalidCredentialsException(string message)
        : base(message, HttpStatusCode.Unauthorized)
    {
    }

    public InvalidCredentialsException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Unauthorized)
    {
    }
}