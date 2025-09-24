using AutoMapper;
using backend.Controllers;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Requests;
using backend.Service;
using backend.Utils.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;

    public AuthControllerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();

        _controller = new AuthController(
            _mockMapper.Object,
            _mockAuthService.Object,
            _mockLogger.Object
        );
    }

    #region Helper Methods

    private void VerifyLogging(string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Register Tests

    [Fact]
    public void Create_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
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

        var userDto = new UserDto
        {
            Id = 1,
            Name = "Test User",
            Email = "test@example.com"
        };

        _mockAuthService.Setup(service => service.Register(request)).Returns(user);
        _mockMapper.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = _controller.Create(request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(URLConstants.Auth + "/register", createdResult.Location);

        var returnValue = Assert.IsType<UserDto>(createdResult.Value);
        Assert.Equal(userDto.Id, returnValue.Id);
        Assert.Equal(userDto.Name, returnValue.Name);
        Assert.Equal(userDto.Email, returnValue.Email);

        _mockAuthService.Verify(service => service.Register(request), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<UserDto>(user), Times.Once);
        VerifyLogging("POST " + URLConstants.Auth + "/register");
    }

    [Fact]
    public void Create_WithNullRequest_ShouldThrowBadRequestException()
    {
        // Arrange
        UserRequest request = null;

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Create(request));
        Assert.Equal("Request cannot be null", exception.Message);

        _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
    }

    [Fact]
    public void Create_WithInvalidRequest_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new UserRequest
        {
            // Missing required fields
        };

        // Add model validation errors
        _controller.ModelState.AddModelError("Name", "Name is required");
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Create(request));
        Assert.Equal("ModelState not valid", exception.Message);

        _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
    }

    [Fact]
    public void Create_WithExistingEmail_ShouldThrowConflictException()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "existing@example.com",
            Password = "password123"
        };

        _mockAuthService.Setup(service => service.Register(request))
            .Throws(new ConflictException($"User with email : {request.Email} already exists"));

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _controller.Create(request));
        Assert.Equal($"User with email : {request.Email} already exists", exception.Message);

        _mockAuthService.Verify(service => service.Register(request), Times.Once);
        VerifyLogging("POST " + URLConstants.Auth + "/register");
    }

    [Fact]
    public void Create_WithServiceException_ShouldPropagateException()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var expectedException = new Exception("Service error");
        _mockAuthService.Setup(service => service.Register(request)).Throws(expectedException);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _controller.Create(request));
        Assert.Same(expectedException, exception);

        _mockAuthService.Verify(service => service.Register(request), Times.Once);
        VerifyLogging("POST " + URLConstants.Auth + "/register");
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "",
            Password = "password123"
        };

        // Add model validation errors
        _controller.ModelState.AddModelError("Email", "Email cannot be empty");

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Create(request));
        Assert.Equal("ModelState not valid", exception.Message);

        _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
    }

    [Fact]
    public void Create_WithEmptyPassword_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = ""
        };

        // Add model validation errors
        _controller.ModelState.AddModelError("Password", "Password cannot be empty");

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Create(request));
        Assert.Equal("ModelState not valid", exception.Message);

        _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
    }

    [Fact]
    public void Create_WithInvalidModelState_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        // Manually add model validation errors to simulate invalid model state
        _controller.ModelState.AddModelError("Name", "Name is required");
        _controller.ModelState.AddModelError("Email", "Email is required");

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Create(request));
        Assert.Equal("ModelState not valid", exception.Message);

        _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
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

        var authResponse = new AuthResponseDto
        {
            Token = "jwt_token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
        };

        _mockAuthService.Setup(service => service.Login(request)).Returns(authResponse);

        // Act
        var result = _controller.Login(request);

        // Assert
        // Since the controller is returning the AuthResponseDto directly, not wrapped in an OkObjectResult
        Assert.Equal(authResponse, result.Value);

        _mockAuthService.Verify(service => service.Login(request), Times.Once);
        VerifyLogging("POST " + URLConstants.Auth + "/login");
    }

    [Fact]
    public void Login_WithNullRequest_ShouldThrowBadRequestException()
    {
        // Arrange
        LoginRequest request = null;

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Login(request));
        Assert.Equal("Request cannot be null", exception.Message);

        _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
    }

    [Fact]
    public void Login_WithInvalidModelState_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Manually add model validation errors to simulate invalid model state
        _controller.ModelState.AddModelError("Email", "Email is required");
        _controller.ModelState.AddModelError("Password", "Password is required");

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Login(request));
        Assert.Equal("ModelState not valid", exception.Message);

        _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
    }

    [Fact]
    public void Login_WithInvalidCredentials_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrong_password"
        };

        _mockAuthService.Setup(service => service.Login(request))
            .Throws(new InvalidCredentialsException("Invalid credentials"));

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Login(request));
        Assert.Equal("Invalid credentials", exception.Message);

        _mockAuthService.Verify(service => service.Login(request), Times.Once);
        VerifyLogging("POST " + URLConstants.Auth + "/login");
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

        _mockAuthService.Setup(service => service.Login(request))
            .Throws(new EntityNotFoundException($"User with email {request.Email} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _controller.Login(request));
        Assert.Equal($"User with email {request.Email} not found", exception.Message);

        _mockAuthService.Verify(service => service.Login(request), Times.Once);
        VerifyLogging("POST " + URLConstants.Auth + "/login");
    }

    [Fact]
    public void Login_WithServiceException_ShouldPropagateException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var expectedException = new Exception("Service error");
        _mockAuthService.Setup(service => service.Login(request)).Throws(expectedException);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() => _controller.Login(request));
        Assert.Same(expectedException, exception);

        _mockAuthService.Verify(service => service.Login(request), Times.Once);
        VerifyLogging("POST " + URLConstants.Auth + "/login");
    }

    [Fact]
    public void Login_WithEmptyEmail_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "",
            Password = "password123"
        };

        // Add model validation errors
        _controller.ModelState.AddModelError("Email", "Email cannot be empty");

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Login(request));
        Assert.Equal("ModelState not valid", exception.Message);

        _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
    }

    [Fact]
    public void Login_WithEmptyPassword_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = ""
        };

        // Add model validation errors
        _controller.ModelState.AddModelError("Password", "Password cannot be empty");

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Login(request));
        Assert.Equal("ModelState not valid", exception.Message);

        _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
    }

    #endregion
}