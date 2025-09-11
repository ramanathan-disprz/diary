using backend.Models;
using backend.Repository;
using backend.Repository.CrudRepository;

namespace backend.Utils;

public static class RepositoryCollection
{
    public static IServiceCollection AddRepositories(this IServiceCollection repositories)
    {
        repositories.AddScoped<ICrudRepository<User>, CrudRepository<User>>();
        return repositories;
    }
}