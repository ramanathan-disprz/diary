namespace backend.Exceptions;

public class InvalidCredentialsException : DisprzException
{
    public InvalidCredentialsException(string message)
        : base(message, 401)
    {
    }

    public InvalidCredentialsException(string message, Exception ex)
        : base(message, 401, ex)
    {
    }
}