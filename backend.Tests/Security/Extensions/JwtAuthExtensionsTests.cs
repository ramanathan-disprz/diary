using backend.Security.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace backend.Tests.Security.Extensions;

public class JwtAuthExtensionsTests
{
    [Fact]
    public void AddJwtAuthentication_WithValidConfiguration_ShouldConfigureJwtAuthentication()
    {
        // Arrange
        var services = new ServiceCollection();

        var configValues = new Dictionary<string, string>
        {
            { "JwtConfig:Secret", "ThisIsAVeryLongSecretKeyForTestingPurposesOnly" },
            { "JwtConfig:Issuer", "TestIssuer" },
            { "JwtConfig:Audience", "TestAudience" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        services.AddJwtAuthentication(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Verify that authentication is registered
        var authenticationSchemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        Assert.NotNull(authenticationSchemeProvider);

        // Verify that JWT Bearer is configured
        var jwtBearerOptions = serviceProvider.GetService<IOptionsMonitor<JwtBearerOptions>>();
        Assert.NotNull(jwtBearerOptions);

        var options = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        Assert.NotNull(options);

        // Verify token validation parameters
        var tokenValidationParameters = options.TokenValidationParameters;
        Assert.NotNull(tokenValidationParameters);
        Assert.True(tokenValidationParameters.ValidateIssuer);
        Assert.True(tokenValidationParameters.ValidateAudience);
        Assert.True(tokenValidationParameters.ValidateLifetime);
        Assert.True(tokenValidationParameters.ValidateIssuerSigningKey);
        Assert.Equal("TestIssuer", tokenValidationParameters.ValidIssuer);
        Assert.Equal("TestAudience", tokenValidationParameters.ValidAudience);
        Assert.NotNull(tokenValidationParameters.IssuerSigningKey);

        // Verify that authorization is registered
        var authorizationOptions =
            serviceProvider.GetService<IOptions<AuthorizationOptions>>();
        Assert.NotNull(authorizationOptions);
    }

    [Fact]
    public void AddJwtAuthentication_WithMissingSecret_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();

        var configValues = new Dictionary<string, string>
        {
            // Missing JwtConfig:Secret
            { "JwtConfig:Issuer", "TestIssuer" },
            { "JwtConfig:Audience", "TestAudience" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddJwtAuthentication(configuration));

        Assert.Equal("JWT Secret is not configured", exception.Message);
    }

    [Fact]
    public void AddJwtAuthentication_ShouldRegisterAuthenticationFailureHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        var configValues = new Dictionary<string, string>
        {
            { "JwtConfig:Secret", "ThisIsAVeryLongSecretKeyForTestingPurposesOnly" },
            { "JwtConfig:Issuer", "TestIssuer" },
            { "JwtConfig:Audience", "TestAudience" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        // Act
        services.AddJwtAuthentication(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var jwtBearerOptions = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        var options = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);

        // Verify that the OnAuthenticationFailed event handler is set
        Assert.NotNull(options.Events);
        Assert.NotNull(options.Events.OnAuthenticationFailed);

        // We can't easily test the actual behavior of the event handler in a unit test
        // But we can verify that it's not null, which confirms it was registered
    }
}