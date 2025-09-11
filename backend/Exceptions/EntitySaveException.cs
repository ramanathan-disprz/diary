namespace backend.Exceptions;

public class EntitySaveException : DisprzException
{
    public EntitySaveException(string message, Exception ex)
        : base(message, 500, ex)
    {
    }
}
