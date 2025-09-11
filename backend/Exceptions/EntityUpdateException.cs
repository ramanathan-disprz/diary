namespace backend.Exceptions;

public class EntityUpdateException: DisprzException
{
    public EntityUpdateException(string message, Exception ex)
        : base(message, 500, ex)
    {
    }
}
