namespace backend.Exceptions;

public class EntityDeleteException : DisprzException
{
    public EntityDeleteException(string message, Exception ex)
        : base(message, 500, ex)
    {
    }
}
