using System.Net;
using backend.Exceptions;
using Xunit;

namespace backend.Tests.Exceptions;

public class EntitySaveExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange
        var message = "Custom entity save error message";

        // Act
        var exception = new EntitySaveException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerExceptionAndStatusCode()
    {
        // Arrange
        var message = "Custom entity save error message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new EntitySaveException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithNoParameters_SetsDefaultMessageAndStatusCode()
    {
        // Act
        var exception = new EntitySaveException();

        // Assert
        Assert.Equal("Entity save failed", exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }
}