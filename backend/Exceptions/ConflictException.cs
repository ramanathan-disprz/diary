using System.Net;

namespace backend.Exceptions;

public class ConflictException : DisprzException
{
    public ConflictException()
        : base("Conflict", HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string message)
        : base(message, HttpStatusCode.Conflict)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.Conflict)
    {
    }
}