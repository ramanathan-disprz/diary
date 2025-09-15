namespace backend.Exceptions;

public class UnauthorizedAccessException : DisprzException
{
    public UnauthorizedAccessException(string message)
        : base(message, 403)
    {
    }

    public UnauthorizedAccessException(string message, Exception ex)
        : base(message, 403, ex)
    {
    }
}