using System.Net;
using backend.Exceptions;
using Xunit;

namespace backend.Tests.Exceptions;

public class DisprzExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessageAndStatusCode()
    {
        // Arrange
        var message = "Test message";

        // Act
        var exception = new TestDisprzException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerExceptionAndStatusCode()
    {
        // Arrange
        var message = "Test message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new TestDisprzException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithNoParameters_SetsStatusCode()
    {
        // Act
        var exception = new TestDisprzException();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
    }

    // Since DisprzException is abstract, we'll test it through a concrete implementation
    private class TestDisprzException : DisprzException
    {
        public TestDisprzException()
            : base(HttpStatusCode.InternalServerError)
        {
        }

        public TestDisprzException(string message)
            : base(message, HttpStatusCode.InternalServerError)
        {
        }

        public TestDisprzException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.InternalServerError)
        {
        }
    }
}