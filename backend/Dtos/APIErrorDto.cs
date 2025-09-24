namespace dotnet_leaner.DTOs;

public class APIErrorDto
{
    public APIErrorDto(string message, int statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }

    public int StatusCode { get; set; }
    public string Message { get; set; }
}