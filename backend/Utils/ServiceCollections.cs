using backend.Service;

namespace backend.Utils;

public static class ServiceCollection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<UserService>();
        services.AddScoped<EventService>();
        return services;
    }
}