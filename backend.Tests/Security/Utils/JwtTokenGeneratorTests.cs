using System.IdentityModel.Tokens.Jwt;
using backend.Models;
using backend.Security.Utils;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace backend.Tests.Security.Utils
{
    public class JwtTokenGeneratorTests
    {
        private readonly IConfiguration _configuration;
        private readonly JwtTokenGenerator _tokenGenerator;
        private readonly User _testUser;

        public JwtTokenGeneratorTests()
        {
            // Set up configuration
            var configValues = new Dictionary<string, string>
            {
                { "JwtConfig:Secret", "ThisIsAVeryLongSecretKeyForTestingPurposesOnly" },
                { "JwtConfig:Issuer", "TestIssuer" },
                { "JwtConfig:Audience", "TestAudience" },
                { "JwtConfig:ExpirationInSeconds", "3600" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            _tokenGenerator = new JwtTokenGenerator(_configuration);

            // Create a test user
            _testUser = new User
            {
                Id = 123,
                Name = "Test User",
                Email = "test@example.com",
                Password = "hashedpassword"
            };
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidToken()
        {
            // Act
            var response = _tokenGenerator.GenerateToken(_testUser);

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response.Token);
            Assert.True(response.ExpiresAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }

        [Fact]
        public void GenerateToken_ShouldIncludeCorrectClaims()
        {
            // Act
            var response = _tokenGenerator.GenerateToken(_testUser);

            // Decode the token to verify claims
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(response.Token);

            // Assert
            Assert.NotNull(token);

            // Check subject claim
            var subClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            Assert.NotNull(subClaim);
            Assert.Equal(_testUser.Id.ToString(), subClaim.Value);

            // Check email claim
            var emailClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
            Assert.NotNull(emailClaim);
            Assert.Equal(_testUser.Email, emailClaim.Value);

            // Check JWT ID claim
            var jtiClaim = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
            Assert.NotNull(jtiClaim);
            Assert.True(Guid.TryParse(jtiClaim.Value, out _));
        }

        [Fact]
        public void GenerateToken_ShouldSetCorrectIssuerAndAudience()
        {
            // Act
            var response = _tokenGenerator.GenerateToken(_testUser);

            // Decode the token
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(response.Token);

            // Assert
            Assert.Equal("TestIssuer", token.Issuer);
            Assert.Equal("TestAudience", token.Audiences.First());
        }

        [Fact]
        public void GenerateToken_ShouldSetCorrectExpiration()
        {
            // Act
            var response = _tokenGenerator.GenerateToken(_testUser);

            // Decode the token
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(response.Token);

            // Assert
            // Token expiration should be approximately 1 hour (3600 seconds) from now
            var expectedExpiration = DateTime.UtcNow.AddSeconds(3600);
            var tokenExpiration = token.ValidTo;

            // Allow a small time difference (5 seconds) due to test execution time
            var timeDifference = Math.Abs((expectedExpiration - tokenExpiration).TotalSeconds);
            Assert.True(timeDifference < 5);

            // Also verify that the ExpiresAt property matches the token expiration
            var tokenExpiresAt = new DateTimeOffset(token.ValidTo).ToUnixTimeSeconds();
            Assert.Equal(tokenExpiresAt, response.ExpiresAt);
        }

        [Fact]
        public void GenerateToken_WithDifferentUser_ShouldGenerateDifferentToken()
        {
            // Arrange
            var user1 = new User
            {
                Id = 123,
                Name = "User One",
                Email = "user1@example.com",
                Password = "hashedpassword"
            };

            var user2 = new User
            {
                Id = 456,
                Name = "User Two",
                Email = "user2@example.com",
                Password = "hashedpassword"
            };

            // Act
            var token1 = _tokenGenerator.GenerateToken(user1);
            var token2 = _tokenGenerator.GenerateToken(user2);

            // Assert
            Assert.NotEqual(token1.Token, token2.Token);
        }

        [Fact]
        public void GenerateToken_CalledTwiceForSameUser_ShouldGenerateDifferentTokens()
        {
            // Act
            var token1 = _tokenGenerator.GenerateToken(_testUser);
            var token2 = _tokenGenerator.GenerateToken(_testUser);

            // Assert
            Assert.NotEqual(token1.Token, token2.Token);

            // This is because each token should have a unique JWT ID (jti claim)
            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadJwtToken(token1.Token);
            var jwtToken2 = handler.ReadJwtToken(token2.Token);

            var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
            var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

            Assert.NotEqual(jti1, jti2);
        }
    }
}