using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Dtos;
using backend.Models;
using Microsoft.IdentityModel.Tokens;

namespace backend.Security.Utils;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _config;

    public JwtTokenGenerator(IConfiguration config)
    {
        _config = config;
    }

    public AuthResponseDto GenerateToken(User user)
    {
        var jwtConfig = _config.GetSection("JwtConfig");
        var secretKey = jwtConfig["Secret"];
        var issuer = jwtConfig["Issuer"];
        var audience = jwtConfig["Audience"];
        var expires = int.Parse(jwtConfig["ExpirationInSeconds"] ?? "3600"); // in seconds

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddSeconds(expires),
            signingCredentials: creds
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = new DateTimeOffset(token.ValidTo).ToUnixTimeSeconds()
        };
    }
}