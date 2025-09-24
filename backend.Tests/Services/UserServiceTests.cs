using AutoMapper;
using backend.Exceptions;
using backend.Models;
using backend.Repository;
using backend.Requests;
using backend.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<UserService>>();

        _service = new UserService(
            _mockMapper.Object,
            _mockLogger.Object,
            _mockRepository.Object
        );
    }

    [Fact]
    public void Index_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Name = "User 1", Email = "user1@example.com", Password = "password1" },
            new() { Id = 2, Name = "User 2", Email = "user2@example.com", Password = "password2" }
        };

        _mockRepository.Setup(repo => repo.FindAll())
            .Returns(users);

        // Act
        var result = _service.Index();

        // Assert
        result.Should().BeEquivalentTo(users);
        _mockRepository.Verify(repo => repo.FindAll(), Times.Once);
    }

    [Fact]
    public void Fetch_ShouldReturnUserById()
    {
        // Arrange
        var userId = 1L;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "password123" };

        _mockRepository.Setup(repo => repo.FindOrThrow(userId))
            .Returns(user);

        // Act
        var result = _service.Fetch(userId);

        // Assert
        result.Should().BeEquivalentTo(user);
        _mockRepository.Verify(repo => repo.FindOrThrow(userId), Times.Once);
    }

    [Fact]
    public void Fetch_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 999L;

        _mockRepository.Setup(repo => repo.FindOrThrow(userId))
            .Throws(new EntityNotFoundException($"User with id {userId} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _service.Fetch(userId));
        exception.Message.Should().Be($"User with id {userId} not found");
        _mockRepository.Verify(repo => repo.FindOrThrow(userId), Times.Once);
    }

    [Fact]
    public void Create_ShouldCreateAndReturnNewUser()
    {
        // Arrange
        var userRequest = new UserRequest
        {
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        var mappedUser = new User
        {
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        var createdUser = new User
        {
            Id = 1,
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        _mockMapper.Setup(mapper => mapper.Map<User>(userRequest))
            .Returns(mappedUser);

        _mockRepository.Setup(repo => repo.Create(It.IsAny<User>()))
            .Returns(createdUser);

        // Act
        var result = _service.Create(userRequest);

        // Assert
        result.Should().BeEquivalentTo(createdUser);
        _mockMapper.Verify(mapper => mapper.Map<User>(userRequest), Times.Once);
        _mockRepository.Verify(repo => repo.Create(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public void Update_ShouldUpdateAndReturnUser()
    {
        // Arrange
        var userId = 1L;
        var userRequest = new UserRequest
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Password = "newpassword123"
        };

        var existingUser = new User
        {
            Id = userId,
            Name = "Original User",
            Email = "original@example.com",
            Password = "oldpassword123"
        };

        var updatedUser = new User
        {
            Id = userId,
            Name = "Updated User",
            Email = "updated@example.com",
            Password = "newpassword123"
        };

        _mockRepository.Setup(repo => repo.FindOrThrow(userId))
            .Returns(existingUser);

        _mockMapper.Setup(mapper => mapper.Map(userRequest, existingUser))
            .Returns(updatedUser);

        _mockRepository.Setup(repo => repo.Update(updatedUser))
            .Returns(updatedUser);

        // Act
        var result = _service.Update(userId, userRequest);

        // Assert
        result.Should().BeEquivalentTo(updatedUser);
        _mockRepository.Verify(repo => repo.FindOrThrow(userId), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map(userRequest, existingUser), Times.Once);
        _mockRepository.Verify(repo => repo.Update(updatedUser), Times.Once);
    }

    [Fact]
    public void Update_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 999L;
        var userRequest = new UserRequest
        {
            Name = "Updated User",
            Email = "updated@example.com",
            Password = "newpassword123"
        };

        _mockRepository.Setup(repo => repo.FindOrThrow(userId))
            .Throws(new EntityNotFoundException($"User with id {userId} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _service.Update(userId, userRequest));
        exception.Message.Should().Be($"User with id {userId} not found");
        _mockRepository.Verify(repo => repo.FindOrThrow(userId), Times.Once);
    }

    [Fact]
    public void Delete_ShouldDeleteUser()
    {
        // Arrange
        var userId = 1L;
        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "password123" };

        _mockRepository.Setup(repo => repo.FindOrThrow(userId))
            .Returns(user);

        _mockRepository.Setup(repo => repo.Delete(user));

        // Act
        _service.Delete(userId);

        // Assert
        _mockRepository.Verify(repo => repo.FindOrThrow(userId), Times.Once);
        _mockRepository.Verify(repo => repo.Delete(user), Times.Once);
    }

    [Fact]
    public void Delete_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 999L;

        _mockRepository.Setup(repo => repo.FindOrThrow(userId))
            .Throws(new EntityNotFoundException($"User with id {userId} not found"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _service.Delete(userId));
        exception.Message.Should().Be($"User with id {userId} not found");
        _mockRepository.Verify(repo => repo.FindOrThrow(userId), Times.Once);
    }

    [Fact]
    public void Create_ShouldGenerateIdForNewUser()
    {
        // Arrange
        var userRequest = new UserRequest
        {
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        var mappedUser = new User
        {
            Name = "New User",
            Email = "newuser@example.com",
            Password = "password123"
        };

        _mockMapper.Setup(mapper => mapper.Map<User>(userRequest))
            .Returns(mappedUser);

        _mockRepository.Setup(repo => repo.Create(It.IsAny<User>()))
            .Returns((User u) => u);

        // Act
        var result = _service.Create(userRequest);

        // Assert
        // We can't directly test the ID value since GenerateId() uses a random number,
        // but we can verify the method was called by checking if the ID is not 0
        result.Id.Should().NotBe(0);
        _mockRepository.Verify(repo => repo.Create(It.Is<User>(u => u.Id != 0)), Times.Once);
    }

    [Fact]
    public void LoggingIsDoneForAllOperations()
    {
        // Arrange
        var userId = 1L;
        var userRequest = new UserRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "password123"
        };

        var user = new User { Id = userId, Name = "Test User", Email = "test@example.com", Password = "password123" };

        _mockRepository.Setup(repo => repo.FindAll()).Returns(new List<User> { user });
        _mockRepository.Setup(repo => repo.FindOrThrow(userId)).Returns(user);
        _mockMapper.Setup(mapper => mapper.Map<User>(userRequest)).Returns(user);
        _mockMapper.Setup(mapper => mapper.Map(userRequest, user)).Returns(user);
        _mockRepository.Setup(repo => repo.Create(It.IsAny<User>())).Returns(user);
        _mockRepository.Setup(repo => repo.Update(It.IsAny<User>())).Returns(user);

        // Act
        _service.Index();
        _service.Fetch(userId);
        _service.Create(userRequest);
        _service.Update(userId, userRequest);
        _service.Delete(userId);

        // Assert
        // Verify that logging occurred for each operation
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Find all users")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Find user with id : {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(3));

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Create new user")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Updating user with id: {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Delete user with id : {userId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}