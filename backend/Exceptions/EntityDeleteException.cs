using System.Net;

namespace backend.Exceptions;

public class EntityDeleteException : DisprzException
{
    public EntityDeleteException()
        : base("Entity deletion failed", HttpStatusCode.InternalServerError)
    {
    }

    public EntityDeleteException(string message)
        : base(message, HttpStatusCode.InternalServerError)
    {
    }

    public EntityDeleteException(string message, Exception innerException)
        : base(message, innerException, HttpStatusCode.InternalServerError)
    {
    }
}