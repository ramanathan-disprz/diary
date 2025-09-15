using backend.Dtos;
using backend.Models;

namespace backend.Security.Utils;

public interface IJwtTokenGenerator
{
    AuthResponseDto GenerateToken(User user);
}