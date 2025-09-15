using backend.Models;
using backend.Repository.CrudRepository;

namespace backend.Repository;

public interface IUserRepository : ICrudRepository<User>
{
    bool ExistsByEmail(string? email);

    User? FindByEmail(string? email);

    User FindByEmailOrThrow(string? email);
}