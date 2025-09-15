using System.Text.Json;
using AutoMapper;
using backend.Models;
using backend.Repository;
using backend.Repository.CrudRepository;
using backend.Requests;

namespace backend.Service;

public class UserService
{
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _log;
    private readonly IUserRepository _repository;

    public UserService(IMapper mapper, ILogger<UserService> log, IUserRepository repository)
    {
        _mapper = mapper;
        _log = log;
        _repository = repository;
    }

    public IEnumerable<User> Index()
    {
        _log.LogInformation("Find all users");
        return _repository.FindAll();
    }

    public User Fetch(long id)
    {
        _log.LogInformation("Find user with id : {userId}", id);
        return _repository.FindOrThrow(id);
    }

    public User Create(UserRequest request)
    {
        _log.LogInformation("Create new user : {userRequest}", JsonSerializer.Serialize(request));
        var user = _mapper.Map<User>(request);
        user.GenerateId();
        return _repository.Create(user);
    }

    public User Update(long id, UserRequest request)
    {
        _log.LogInformation("Updating user with id: {UserId} " +
                            "and request: {userRequest}", id, JsonSerializer.Serialize(request));
        var user = Fetch(id);
        user = _mapper.Map(request, user);
        return _repository.Update(user);
    }

    public void Delete(long id)
    {
        _log.LogInformation("Delete user with id : {userId}", id);
        var user = Fetch(id);
        _repository.Delete(user);
    }
}