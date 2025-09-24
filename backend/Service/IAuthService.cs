using backend.Dtos;
using backend.Models;
using backend.Requests;

namespace backend.Service;

public interface IAuthService
{
    User Register(UserRequest request);
    
    AuthResponseDto Login(LoginRequest request);
}