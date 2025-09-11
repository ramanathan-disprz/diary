namespace backend.Exceptions;

public class DisprzException : Exception
{
    public int StatusCode { get; set; }
    public Exception Exception { get; set; }

    public DisprzException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public DisprzException(string message, int statusCode, Exception ex) : base(message)
    {
        StatusCode = statusCode;
        Exception = ex;
    }
}
