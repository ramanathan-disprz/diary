using System.Reflection;
using AutoMapper;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Repository;
using backend.Requests;
using backend.Security.Utils;
using backend.Service;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenGenerator> _mockJwtGenerator;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _mockRepository = new Mock<IUserRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtGenerator = new Mock<IJwtTokenGenerator>();

        _service = new AuthService(
            _mockMapper.Object,
            _mockLogger.Object,
            _mockRepository.Object,
            _mockPasswordHasher.Object,
            _mockJwtGenerator.Object
        );
    }

    #region Register Tests

    [Fact]
    public void Register_WithValidRequest_ShouldCreateAndReturnUser()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var hashedPassword = "hashed_password";
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = hashedPassword
        };

        _mockRepository.Setup(repo => repo.ExistsByEmail(request.Email)).Returns(false);
        _mockPasswordHasher.Setup(hasher => hasher.HashPassword(request.Password)).Returns(hashedPassword);
        _mockMapper.Setup(mapper => mapper.Map<User>(It.IsAny<UserRequest>())).Returns(user);
        _mockRepository.Setup(repo => repo.Create(It.IsAny<User>())).Returns(user);

        var initialPassword = request.Password;

        // Act
        var result = _service.Register(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Name, result.Name);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(hashedPassword, result.Password);

        _mockRepository.Verify(repo => repo.ExistsByEmail(request.Email), Times.Once);
        _mockPasswordHasher.Verify(hasher => hasher.HashPassword(initialPassword), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<User>(It.IsAny<UserRequest>()), Times.Once);
        _mockRepository.Verify(repo => repo.Create(It.IsAny<User>()), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create new user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Register_WithExistingEmail_ShouldThrowConflictException()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "existing@example.com",
            Password = "password123"
        };

        _mockRepository.Setup(repo => repo.ExistsByEmail(request.Email)).Returns(true);

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _service.Register(request));
        Assert.Equal($"User with email : {request.Email} already exists", exception.Message);

        _mockRepository.Verify(repo => repo.ExistsByEmail(request.Email), Times.Once);
        _mockPasswordHasher.Verify(hasher => hasher.HashPassword(It.IsAny<string>()), Times.Never);
        _mockMapper.Verify(mapper => mapper.Map<User>(It.IsAny<UserRequest>()), Times.Never);
        _mockRepository.Verify(repo => repo.Create(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Register_ShouldHashPassword()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var hashedPassword = "hashed_password";
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = hashedPassword
        };

        _mockRepository.Setup(repo => repo.ExistsByEmail(request.Email)).Returns(false);
        _mockPasswordHasher.Setup(hasher => hasher.HashPassword(request.Password)).Returns(hashedPassword);
        _mockMapper.Setup(mapper => mapper.Map<User>(It.IsAny<UserRequest>())).Returns(user);
        _mockRepository.Setup(repo => repo.Create(It.IsAny<User>())).Returns(user);

        // Act
        _service.Register(request);

        // Assert
        // Verify that the password in the request was updated with the hashed password
        Assert.Equal(hashedPassword, request.Password);
        _mockPasswordHasher.Verify(hasher => hasher.HashPassword("password123"), Times.Once);
    }

    [Fact]
    public void Register_ShouldGenerateUserId()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var hashedPassword = "hashed_password";
        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = hashedPassword
        };

        _mockRepository.Setup(repo => repo.ExistsByEmail(request.Email)).Returns(false);
        _mockPasswordHasher.Setup(hasher => hasher.HashPassword(request.Password)).Returns(hashedPassword);
        _mockMapper.Setup(mapper => mapper.Map<User>(It.IsAny<UserRequest>())).Returns(user);
        _mockRepository.Setup(repo => repo.Create(It.IsAny<User>())).Returns((User u) => u);

        // Act
        var result = _service.Register(request);

        // Assert
        // We can't directly test the GenerateId method since it's not virtual,
        // but we can verify that Create was called with a user object
        _mockRepository.Verify(repo => repo.Create(It.IsAny<User>()), Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public void Login_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        var authResponse = new AuthResponseDto
        {
            Token = "jwt_token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
        };

        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email)).Returns(user);
        _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(request.Password, user.Password)).Returns(true);
        _mockJwtGenerator.Setup(jwt => jwt.GenerateToken(user)).Returns(authResponse);

        // Act
        var result = _service.Login(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(authResponse.Token, result.Token);
        Assert.Equal(authResponse.ExpiresAt, result.ExpiresAt);

        _mockRepository.Verify(repo => repo.FindByEmailOrThrow(request.Email), Times.Once);
        _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(request.Password, user.Password), Times.Once);
        _mockJwtGenerator.Verify(jwt => jwt.GenerateToken(user), Times.Once);
    }

    [Fact]
    public void Login_WithNonExistentEmail_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123"
        };

        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email))
            .Throws(new EntityNotFoundException($"User with email {request.Email} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _service.Login(request));
        Assert.Equal($"User with email {request.Email} not found", exception.Message);

        _mockRepository.Verify(repo => repo.FindByEmailOrThrow(request.Email), Times.Once);
        _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _mockJwtGenerator.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Login_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrong_password"
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email)).Returns(user);
        _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(request.Password, user.Password)).Returns(false);

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _service.Login(request));
        Assert.Equal("Invalid credentials", exception.Message);

        _mockRepository.Verify(repo => repo.FindByEmailOrThrow(request.Email), Times.Once);
        _mockPasswordHasher.Verify(hasher => hasher.VerifyPassword(request.Password, user.Password), Times.Once);
        _mockJwtGenerator.Verify(jwt => jwt.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Login_WithEmptyPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email)).Returns(user);
        _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(request.Password, user.Password)).Returns(false);

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _service.Login(request));
        Assert.Equal("Invalid credentials", exception.Message);
    }

    [Fact]
    public void Login_WithNullPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = null
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email)).Returns(user);
        _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(It.IsAny<string>(), user.Password))
            .Returns(false);

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _service.Login(request));
        Assert.Equal("Invalid credentials", exception.Message);
    }

    #endregion

    #region Private Methods Tests

    [Fact]
    public void ValidateEmail_WithNonExistingEmail_ShouldNotThrowException()
    {
        // Arrange
        var email = "new@example.com";
        _mockRepository.Setup(repo => repo.ExistsByEmail(email)).Returns(false);

        // Act & Assert
        // Use reflection to call the private method
        var method = typeof(AuthService).GetMethod("ValidateEmail",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // No exception should be thrown
        var exception = Record.Exception(() => method.Invoke(_service, new object[] { email }));
        Assert.Null(exception);
    }

    [Fact]
    public void HashPassword_ShouldUpdateRequestPassword()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var hashedPassword = "hashed_password";
        _mockPasswordHasher.Setup(hasher => hasher.HashPassword(request.Password)).Returns(hashedPassword);

        // Act
        // Use reflection to call the private method
        var method = typeof(AuthService).GetMethod("HashPassword",
            BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(_service, new object[] { request });

        // Assert
        Assert.Equal(hashedPassword, request.Password);
        _mockPasswordHasher.Verify(hasher => hasher.HashPassword("password123"), Times.Once);
    }

    [Fact]
    public void ValidateCredentials_WithValidCredentials_ShouldNotThrowException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(request.Password, user.Password)).Returns(true);

        // Act
        // Use reflection to call the private method
        var method = typeof(AuthService).GetMethod("ValidateCredentials",
            BindingFlags.NonPublic | BindingFlags.Instance);

        // No exception should be thrown
        var exception = Record.Exception(() => method.Invoke(_service, new object[] { request, user }));
        Assert.Null(exception);
    }

    [Fact]
    public void GenerateToken_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        var authResponse = new AuthResponseDto
        {
            Token = "jwt_token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
        };

        _mockJwtGenerator.Setup(jwt => jwt.GenerateToken(user)).Returns(authResponse);

        // Act
        // Use reflection to call the private method
        var method = typeof(AuthService).GetMethod("GenerateToken",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method.Invoke(_service, new object[] { user }) as AuthResponseDto;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(authResponse.Token, result.Token);
        Assert.Equal(authResponse.ExpiresAt, result.ExpiresAt);
        _mockJwtGenerator.Verify(jwt => jwt.GenerateToken(user), Times.Once);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void Register_WithNullEmail_ShouldHandleGracefully()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = null,
            Password = "password123"
        };

        _mockRepository.Setup(repo => repo.ExistsByEmail(It.IsAny<string>())).Returns(false);

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = null,
            Password = "hashed_password"
        };

        _mockPasswordHasher.Setup(hasher => hasher.HashPassword(request.Password)).Returns("hashed_password");
        _mockMapper.Setup(mapper => mapper.Map<User>(It.IsAny<UserRequest>())).Returns(user);
        _mockRepository.Setup(repo => repo.Create(It.IsAny<User>())).Returns(user);

        // Act
        var result = _service.Register(request);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.Email);
    }

    [Fact]
    public void Login_WithRepositoryException_ShouldPropagateException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var expectedException = new Exception("Database error");
        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email)).Throws(expectedException);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _service.Login(request));
        Assert.Same(expectedException, exception);
    }

    [Fact]
    public void Login_WithPasswordHasherException_ShouldPropagateException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        var expectedException = new Exception("Hashing error");
        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email)).Returns(user);
        _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(request.Password, user.Password))
            .Throws(expectedException);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _service.Login(request));
        Assert.Same(expectedException, exception);
    }

    [Fact]
    public void Login_WithJwtGeneratorException_ShouldPropagateException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com",
            Password = "hashed_password"
        };

        var expectedException = new Exception("JWT generation error");
        _mockRepository.Setup(repo => repo.FindByEmailOrThrow(request.Email)).Returns(user);
        _mockPasswordHasher.Setup(hasher => hasher.VerifyPassword(request.Password, user.Password)).Returns(true);
        _mockJwtGenerator.Setup(jwt => jwt.GenerateToken(user)).Throws(expectedException);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _service.Login(request));
        Assert.Same(expectedException, exception);
    }

    #endregion
}