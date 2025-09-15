using backend.Security.Filters;
using backend.Security.Utils;
using backend.Service;

namespace backend.Utils;

public static class ServiceCollection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<JwtAuthFilter>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<AuthService>();
        
        services.AddScoped<UserService>();
        services.AddScoped<EventService>();

        return services;
    }
}