using System.Net;

namespace dotnet_leaner.DTOs;

public class APIErrorDto
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public APIErrorDto(string message, int statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }
}
