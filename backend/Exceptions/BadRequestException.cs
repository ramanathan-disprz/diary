namespace backend.Exceptions;

public class BadRequestException : DisprzException
{
    public BadRequestException(string message)
        : base(message, 400)
    {
    }

    public BadRequestException(string message, Exception ex)
        : base(message, 400, ex)
    {
    }
}