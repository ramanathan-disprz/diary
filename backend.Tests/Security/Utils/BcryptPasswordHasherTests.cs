using backend.Security.Utils;
using BCrypt.Net;
using Xunit;

namespace backend.Tests.Security.Utils;

public class BcryptPasswordHasherTests
{
    private readonly BcryptPasswordHasher _passwordHasher;

    public BcryptPasswordHasherTests()
    {
        _passwordHasher = new BcryptPasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "SecurePassword123";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
        Assert.StartsWith("$2", hashedPassword); // BCrypt hash format starts with $2a$ or $2b$
    }

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldReturnHash()
    {
        // Arrange
        var password = "";

        // Act
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "SecurePassword123";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123";
        var incorrectPassword = "WrongPassword123";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "SecurePassword123";
        var emptyPassword = "";
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(emptyPassword, hashedPassword);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldHandleGracefully()
    {
        // Arrange
        var password = "SecurePassword123";
        var invalidHash = "not-a-valid-bcrypt-hash";

        // Act & Assert
        // This should either return false or throw a specific exception
        // depending on how BCrypt.Net.BCrypt.Verify handles invalid hashes
        var exception = Record.Exception(() => _passwordHasher.VerifyPassword(password, invalidHash));

        // If it throws, it should be a specific exception type from BCrypt
        if (exception != null)
        {
            Assert.IsType<SaltParseException>(exception);
        }
        // If it doesn't throw, it should return false
        else
        {
            var result = _passwordHasher.VerifyPassword(password, invalidHash);
            Assert.False(result);
        }
    }
}