using System.Diagnostics.CodeAnalysis;
using backend.Security.Filters;
using backend.Security.Utils;
using backend.Service;

namespace backend.Utils;
    [ExcludeFromCodeCoverage]

public static class ServiceCollection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<JwtAuthFilter>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEventService, EventService>();

        return services;
    }
}