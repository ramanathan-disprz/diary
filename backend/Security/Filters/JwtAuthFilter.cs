using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Exceptions;
using backend.Security.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace backend.Security.Filters;

public class JwtAuthFilter : IAsyncAuthorizationFilter
{
    private readonly ILogger<JwtAuthFilter> _logger;

    public JwtAuthFilter(ILogger<JwtAuthFilter> logger)
    {
        _logger = logger;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // Allow anonymous endpoints
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            return Task.CompletedTask;
        }

        // Ensure authentication middleware already ran
        var user = context.HttpContext.User;
        if (!(user?.Identity?.IsAuthenticated ?? false))
        {
            _logger.LogInformation("Request is not authenticated - rejecting.");
            throw new InvalidCredentialsException("Token invalid or missing");
        }

        // Extract user id
        var sub = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(sub) || !long.TryParse(sub, out var userId))
        {
            _logger.LogWarning("Authenticated principal missing numeric 'sub' or NameIdentifier claim.");
            throw new MalformedJwtException("Authenticated user missing numeric 'sub' claim");
        }

        // Save to HttpContext.Items for downstream use
        context.HttpContext.Items[HttpContextItemKeys.UserId] = userId;

        return Task.CompletedTask;
    }
}