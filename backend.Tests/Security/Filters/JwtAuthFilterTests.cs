using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Exceptions;
using backend.Security.Constants;
using backend.Security.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Security.Filters;

public class JwtAuthFilterTests
{
    private readonly JwtAuthFilter _filter;
    private readonly Mock<ILogger<JwtAuthFilter>> _mockLogger;

    public JwtAuthFilterTests()
    {
        _mockLogger = new Mock<ILogger<JwtAuthFilter>>();
        _filter = new JwtAuthFilter(_mockLogger.Object);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WithAllowAnonymous_ShouldAllowAccess()
    {
        // Arrange
        var context = CreateAuthorizationFilterContext(true, false);

        // Act
        await _filter.OnAuthorizationAsync(context);

        // Assert
        // No exception should be thrown
        // No items should be added to HttpContext.Items
        Assert.Empty(context.HttpContext.Items);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WithUnauthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var context = CreateAuthorizationFilterContext(false, false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _filter.OnAuthorizationAsync(context));

        Assert.Equal("Token invalid or missing", exception.Message);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WithAuthenticatedUserButNoSubClaim_ShouldThrowMalformedJwtException()
    {
        // Arrange
        var context = CreateAuthorizationFilterContext(false, true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MalformedJwtException>(() =>
            _filter.OnAuthorizationAsync(context));

        Assert.Equal("Authenticated user missing numeric 'sub' claim", exception.Message);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WithAuthenticatedUserAndValidSubClaim_ShouldAddUserIdToContext()
    {
        // Arrange
        var context = CreateAuthorizationFilterContext(false, true, "123");

        // Act
        await _filter.OnAuthorizationAsync(context);

        // Assert
        Assert.True(context.HttpContext.Items.ContainsKey(HttpContextItemKeys.UserId));
        Assert.Equal(123L, context.HttpContext.Items[HttpContextItemKeys.UserId]);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WithAuthenticatedUserAndNameIdentifierClaim_ShouldAddUserIdToContext()
    {
        // Arrange
        var context = CreateAuthorizationFilterContext(false, true, null, "456");

        // Act
        await _filter.OnAuthorizationAsync(context);

        // Assert
        Assert.True(context.HttpContext.Items.ContainsKey(HttpContextItemKeys.UserId));
        Assert.Equal(456L, context.HttpContext.Items[HttpContextItemKeys.UserId]);
    }

    [Fact]
    public async Task OnAuthorizationAsync_WithNonNumericSubClaim_ShouldThrowMalformedJwtException()
    {
        // Arrange
        var context = CreateAuthorizationFilterContext(false, true, "not-a-number");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MalformedJwtException>(() =>
            _filter.OnAuthorizationAsync(context));

        Assert.Equal("Authenticated user missing numeric 'sub' claim", exception.Message);
    }

    private AuthorizationFilterContext CreateAuthorizationFilterContext(
        bool isAnonymous,
        bool isAuthenticated,
        string subClaim = null,
        string nameIdentifierClaim = null)
    {
        // Create HttpContext
        var httpContext = new DefaultHttpContext();

        // Set up endpoint with AllowAnonymous if needed
        if (isAnonymous)
        {
            var endpointMetadata = new List<object> { new AllowAnonymousAttribute() };
            var endpoint = new Endpoint(
                context => Task.CompletedTask,
                new EndpointMetadataCollection(endpointMetadata),
                "Test Endpoint");

            httpContext.SetEndpoint(endpoint);
        }

        // Set up authenticated user with claims if needed
        if (isAuthenticated)
        {
            var claims = new List<Claim>();

            if (subClaim != null) claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subClaim));

            if (nameIdentifierClaim != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifierClaim));

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);

            httpContext.User = principal;
        }

        // Create ActionContext
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        // Create AuthorizationFilterContext
        return new AuthorizationFilterContext(
            actionContext,
            new List<IFilterMetadata>());
    }
}