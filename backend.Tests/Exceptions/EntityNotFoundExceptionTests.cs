using System.Net;
using backend.Exceptions;
using Xunit;

namespace backend.Tests.Exceptions;

public class EntityNotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange
        var message = "Custom entity not found message";

        // Act
        var exception = new EntityNotFoundException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerExceptionAndStatusCode()
    {
        // Arrange
        var message = "Custom entity not found message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new EntityNotFoundException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithId_SetsMessageWithIdAndStatusCode()
    {
        // Arrange
        var id = 123;

        // Act
        var exception = new EntityNotFoundException(id);

        // Assert
        Assert.Equal($"Entity with id {id} not found", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithNoParameters_SetsDefaultMessageAndStatusCode()
    {
        // Act
        var exception = new EntityNotFoundException();

        // Assert
        Assert.Equal("Entity not found", exception.Message);
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }
}