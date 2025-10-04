using System.Diagnostics.CodeAnalysis;
using backend.Repository;

namespace backend.Utils;

[ExcludeFromCodeCoverage]
public static class RepositoryCollection
{
    public static IServiceCollection AddRepositories(this IServiceCollection repositories)
    {
        repositories.AddScoped<IUserRepository, UserRepository>();
        repositories.AddScoped<IEventRepository, EventRepository>();
        return repositories;
    }
}