using backend.Models;
using backend.Requests;

namespace backend.Service;

public interface IUserService
{
    IEnumerable<User> Index();

    User Fetch(long id);

    User Create(UserRequest request);

    User Update(long id, UserRequest request);

    void Delete(long id);
}