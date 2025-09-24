using System.Net;

namespace backend.Exceptions;

public class BadRequestException : DisprzException
{
    public BadRequestException()
        : base("Bad Request", HttpStatusCode.BadRequest)
    {
    }

    public BadRequestException(string message)
        : base(message, HttpStatusCode.BadRequest)
    {
    }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.BadRequest)
    {
    }
}