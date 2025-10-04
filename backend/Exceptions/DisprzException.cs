using System.Net;

namespace backend.Exceptions;

public class DisprzException : Exception
{
    protected DisprzException(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }


    protected DisprzException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    protected DisprzException(string message, Exception innerException, HttpStatusCode statusCode)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}