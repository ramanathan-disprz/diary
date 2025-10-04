using System.Text.Json;
using AutoMapper;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Repository;
using backend.Requests;
using backend.Security.Utils;

namespace backend.Service;

public class AuthService : IAuthService
{
    private readonly IJwtTokenGenerator _jwt;
    private readonly ILogger<AuthService> _log;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _repository;

    public AuthService(IMapper mapper, ILogger<AuthService> log, IUserRepository repository,
        IPasswordHasher passwordHasher, IJwtTokenGenerator jwt)
    {
        _log = log;
        _jwt = jwt;
        _mapper = mapper;
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public User Register(UserRequest request)
    {
        _log.LogInformation("Create new user : {userRequest}", JsonSerializer.Serialize(request));
        ValidateEmail(request.Email);
        HashPassword(request);
        var user = _mapper.Map<User>(request);
        user.GenerateId();
        return _repository.Create(user);
    }

    public AuthResponseDto Login(LoginRequest request)
    {
        var user = _repository.FindByEmailOrThrow(request.Email);
        ValidateCredentials(request, user);
        return GenerateToken(user);
    }

    private void ValidateEmail(string? email)
    {
        var existsByEmail = _repository.ExistsByEmail(email);
        if (existsByEmail)
            throw new ConflictException($"User with email : {email} already exists");
    }

    private void HashPassword(UserRequest request)
    {
        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        request.Password = hashedPassword;
    }

    private void ValidateCredentials(LoginRequest request, User user)
    {
        if (!_passwordHasher.VerifyPassword(request.Password, user.Password))
            throw new InvalidCredentialsException("Invalid credentials");
    }

    private AuthResponseDto GenerateToken(User user)
    {
        var jwt = _jwt.GenerateToken(user);
        return jwt;
    }
}