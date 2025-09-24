using System.Net;

namespace backend.Exceptions;

public class EntityUpdateException : DisprzException
{
    public EntityUpdateException()
        : base("Entity update failed", HttpStatusCode.InternalServerError)
    {
    }

    public EntityUpdateException(string message)
        : base(message, HttpStatusCode.InternalServerError)
    {
    }

    public EntityUpdateException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.InternalServerError)
    {
    }
}