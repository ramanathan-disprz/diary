namespace backend.Exceptions;

public class ConflictException : DisprzException
{
    public ConflictException(string message)
        : base(message, 409)
    {
    }

    public ConflictException(string message, Exception ex)
        : base(message, 409, ex)
    {
    }
}