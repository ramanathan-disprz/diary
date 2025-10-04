using System.Net;
using System.Text.Json;
using dotnet_leaner.DTOs;

namespace backend.Exceptions;

public class GlobalExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (DisprzException ex)
        {
            _logger.LogError("Handled Disprz exception: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(httpContext, HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new APIErrorDto(message, (int)statusCode);

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}