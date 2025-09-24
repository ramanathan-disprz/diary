using System.Net;
using System.Text.Json;
using backend.Exceptions;
using dotnet_leaner.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Exceptions;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;
    private readonly DefaultHttpContext _httpContext;
    private readonly Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
    private readonly MemoryStream _responseBody;

    public GlobalExceptionHandlerTests()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();

        // Create a dummy RequestDelegate that throws an exception
        RequestDelegate next = context =>
            throw new Exception("Test exception");

        _handler = new GlobalExceptionHandler(next, _mockLogger.Object);

        _httpContext = new DefaultHttpContext();
        _responseBody = new MemoryStream();
        _httpContext.Response.Body = _responseBody;
    }

    [Fact]
    public async Task InvokeAsync_WithDisprzException_ReturnsCorrectStatusCodeAndMessage()
    {
        // Arrange
        var expectedStatusCode = HttpStatusCode.BadRequest;
        var expectedMessage = "Test bad request";

        // Create a dummy RequestDelegate that throws a DisprzException
        RequestDelegate next = context =>
            throw new BadRequestException(expectedMessage);

        var handler = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await handler.InvokeAsync(_httpContext);

        // Assert
        _responseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(_responseBody).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<APIErrorDto>(responseBodyText);

        Assert.Equal((int)expectedStatusCode, _httpContext.Response.StatusCode);
        Assert.Equal("application/json", _httpContext.Response.ContentType);
        Assert.Equal(expectedMessage, response.Message);
        Assert.Equal((int)expectedStatusCode, response.StatusCode);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Handled Disprz exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var expectedStatusCode = HttpStatusCode.InternalServerError;
        var expectedMessage = "An unexpected error occurred.";

        // Act
        await _handler.InvokeAsync(_httpContext);

        // Assert
        _responseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(_responseBody).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<APIErrorDto>(responseBodyText);

        Assert.Equal((int)expectedStatusCode, _httpContext.Response.StatusCode);
        Assert.Equal("application/json", _httpContext.Response.ContentType);
        Assert.Equal(expectedMessage, response.Message);
        Assert.Equal((int)expectedStatusCode, response.StatusCode);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled exception")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_CompletesSuccessfully()
    {
        // Arrange
        // Create a dummy RequestDelegate that doesn't throw an exception
        RequestDelegate next = context => Task.CompletedTask;

        var handler = new GlobalExceptionHandler(next, _mockLogger.Object);

        // Act
        await handler.InvokeAsync(_httpContext);

        // Assert
        // No exception should be thrown
        // No logging should occur
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }
}