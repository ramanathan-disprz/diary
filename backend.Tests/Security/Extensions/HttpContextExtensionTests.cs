using backend.Security.Constants;
using backend.Security.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace backend.Tests.Security.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetUserId_WithUserIdInItems_ShouldReturnUserId()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var expectedUserId = 123L;
        httpContext.Items[HttpContextItemKeys.UserId] = expectedUserId;

        // Act
        var result = httpContext.GetUserId();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUserId, result);
    }

    [Fact]
    public void GetUserId_WithNoUserIdInItems_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        // No UserId in Items

        // Act
        var result = httpContext.GetUserId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_WithNonLongUserIdInItems_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items[HttpContextItemKeys.UserId] = "not a long"; // String instead of long

        // Act
        var result = httpContext.GetUserId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_WithEmptyItems_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        // Items collection is empty by default

        // Act
        var result = httpContext.GetUserId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_WithOtherItemsButNoUserId_ShouldReturnNull()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Items["SomeOtherKey"] = "SomeValue";
        // No UserId in Items

        // Act
        var result = httpContext.GetUserId();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void HttpContextItemKeys_UserId_ShouldHaveCorrectValue()
    {
        // This test verifies the constant value in HttpContextItemKeys
        Assert.Equal("UserId", HttpContextItemKeys.UserId);
    }
}