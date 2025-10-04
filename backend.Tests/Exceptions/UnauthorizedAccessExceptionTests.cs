using System.Net;
using Xunit;
using UnauthorizedAccessException = backend.Exceptions.UnauthorizedAccessException;

namespace backend.Tests.Exceptions;

public class UnauthorizedAccessExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange
        var message = "Custom unauthorized access message";

        // Act
        var exception = new UnauthorizedAccessException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerExceptionAndStatusCode()
    {
        // Arrange
        var message = "Custom unauthorized access message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new UnauthorizedAccessException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithNoParameters_SetsDefaultMessageAndStatusCode()
    {
        // Act
        var exception = new UnauthorizedAccessException();

        // Assert
        Assert.Equal("Unauthorized access", exception.Message);
        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }
}