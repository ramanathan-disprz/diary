using AutoMapper;
using backend.Controllers;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Requests;
using backend.Security.Constants;
using backend.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class UserControllerTests
{
    private readonly UserController _controller;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUserService> _mockUserService;

    public UserControllerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();

        // Setup controller with HttpContext
        _controller = new UserController(
            _mockMapper.Object,
            _mockUserService.Object,
            _mockLogger.Object
        );

        // Setup HttpContext
        var httpContext = new DefaultHttpContext();
        httpContext.Items = new Dictionary<object, object>();
        httpContext.Items[HttpContextItemKeys.UserId] = 1L;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region Index Tests

    [Fact]
    public void Index_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "User 1", Email = "user1@example.com", Password = "hashed_password" },
            new() { Id = 2, Name = "User 2", Email = "user2@example.com", Password = "hashed_password" }
        };

        var userDtos = new List<UserDto>
        {
            new() { Id = 1, Name = "User 1", Email = "user1@example.com" },
            new() { Id = 2, Name = "User 2", Email = "user2@example.com" }
        };

        _mockUserService.Setup(service => service.Index()).Returns(users);
        _mockMapper.Setup(mapper => mapper.Map<IEnumerable<UserDto>>(users)).Returns(userDtos);

        // Act
        var result = _controller.Index();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count());

        _mockUserService.Verify(service => service.Index(), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<IEnumerable<UserDto>>(users), Times.Once);

        // Verify the actual log message format used in the controller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GET /v1/users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Fetch Tests

    [Fact]
    public void Fetch_ShouldReturnCurrentUser()
    {
        // Arrange
        var userId = 1L;
        var user = new User
            { Id = userId, Name = "Test User", Email = "test@example.com", Password = "hashed_password" };
        var userDto = new UserDto { Id = userId, Name = "Test User", Email = "test@example.com" };

        _mockUserService.Setup(service => service.Fetch(userId)).Returns(user);
        _mockMapper.Setup(mapper => mapper.Map<UserDto>(user)).Returns(userDto);

        // Act
        var result = _controller.Fetch();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(userDto.Id, returnValue.Id);
        Assert.Equal(userDto.Name, returnValue.Name);
        Assert.Equal(userDto.Email, returnValue.Email);

        _mockUserService.Verify(service => service.Fetch(userId), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<UserDto>(user), Times.Once);

        // Verify the actual log message format used in the controller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GET /v1/users/me")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Fetch_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Fetch());
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockUserService.Verify(service => service.Fetch(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public void Fetch_WithNonExistentUser_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 1L;
        _mockUserService.Setup(service => service.Fetch(userId))
            .Throws(new EntityNotFoundException($"User with id {userId} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _controller.Fetch());
        Assert.Equal($"User with id {userId} not found", exception.Message);

        _mockUserService.Verify(service => service.Fetch(userId), Times.Once);

        // Verify the actual log message format used in the controller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GET /v1/users/me")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_ShouldUpdateAndReturnUser()
    {
        // Arrange
        var userId = 1L;
        var request = new UserRequest
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Password = "new_password"
        };

        var updatedUser = new User
        {
            Id = userId,
            Name = "Updated User",
            Email = "updated@example.com",
            Password = "hashed_new_password"
        };

        var userDto = new UserDto
        {
            Id = userId,
            Name = "Updated User",
            Email = "updated@example.com"
        };

        _mockUserService.Setup(service => service.Update(userId, request)).Returns(updatedUser);
        _mockMapper.Setup(mapper => mapper.Map<UserDto>(updatedUser)).Returns(userDto);

        // Act
        var result = _controller.Update(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(userDto.Id, returnValue.Id);
        Assert.Equal(userDto.Name, returnValue.Name);
        Assert.Equal(userDto.Email, returnValue.Email);

        _mockUserService.Verify(service => service.Update(userId, request), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<UserDto>(updatedUser), Times.Once);

        // Verify the actual log message format used in the controller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PUT /v1/users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Update_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new UserRequest
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Password = "new_password"
        };

        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Update(request));
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockUserService.Verify(service => service.Update(It.IsAny<long>(), It.IsAny<UserRequest>()), Times.Never);
    }

    [Fact]
    public void Update_WithNonExistentUser_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 1L;
        var request = new UserRequest
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Password = "new_password"
        };

        _mockUserService.Setup(service => service.Update(userId, request))
            .Throws(new EntityNotFoundException($"User with id {userId} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _controller.Update(request));
        Assert.Equal($"User with id {userId} not found", exception.Message);

        _mockUserService.Verify(service => service.Update(userId, request), Times.Once);

        // Verify the actual log message format used in the controller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PUT /v1/users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldDeleteUserAndReturnNoContent()
    {
        // Arrange
        var userId = 1L;
        _mockUserService.Setup(service => service.Delete(userId));

        // Act
        var result = _controller.Delete();

        // Assert
        Assert.IsType<NoContentResult>(result);

        _mockUserService.Verify(service => service.Delete(userId), Times.Once);

        // Verify the actual log message format used in the controller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DELETE /v1/users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Delete_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Delete());
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockUserService.Verify(service => service.Delete(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public void Delete_WithNonExistentUser_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 1L;
        _mockUserService.Setup(service => service.Delete(userId))
            .Throws(new EntityNotFoundException($"User with id {userId} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _controller.Delete());
        Assert.Equal($"User with id {userId} not found", exception.Message);

        _mockUserService.Verify(service => service.Delete(userId), Times.Once);

        // Verify the actual log message format used in the controller
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DELETE /v1/users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion
}