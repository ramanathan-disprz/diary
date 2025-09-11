namespace backend.Exceptions;

public class EntityNotFoundException : DisprzException
{
    public EntityNotFoundException(string message)
        : base(message, 404)
    {
    }

    public EntityNotFoundException(string message, Exception ex)
        : base(message, 404, ex)
    {
    }
}
