using System.Net;

namespace backend.Exceptions;

public class EntitySaveException : DisprzException
{
    public EntitySaveException()
        : base("Entity save failed", HttpStatusCode.InternalServerError)
    {
    }

    public EntitySaveException(string message)
        : base(message, HttpStatusCode.InternalServerError)
    {
    }

    public EntitySaveException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.InternalServerError)
    {
    }
}