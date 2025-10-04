using System.Net;
using backend.Exceptions;
using Xunit;

namespace backend.Tests.Exceptions;

public class MalformedJwtExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange
        var message = "Custom malformed JWT message";

        // Act
        var exception = new MalformedJwtException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerExceptionAndStatusCode()
    {
        // Arrange
        var message = "Custom malformed JWT message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new MalformedJwtException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithNoParameters_SetsDefaultMessageAndStatusCode()
    {
        // Act
        var exception = new MalformedJwtException();

        // Assert
        Assert.Equal("Malformed JWT token", exception.Message);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
    }
}