namespace backend.Exceptions;

public class MalformedJwtException : DisprzException
{
    public MalformedJwtException(string message)
        : base(message, 401)
    {
    }

    public MalformedJwtException(string message, Exception ex)
        : base(message, 401, ex)
    {
    }
}