using backend.Data;
using backend.Exceptions;
using backend.Models;
using backend.Repository.CrudRepository;

namespace backend.Repository;

public class UserRepository : CrudRepository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public bool ExistsByEmail(string? email)
    {
        return _context.Users.Any(u => u.Email == email);
    }

    public User? FindByEmail(string? email)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email);
    }

    public User FindByEmailOrThrow(string? email)
    {
        return FindByEmail(email) ??
               throw new EntityNotFoundException($"User with email {email} not found");
    }
}