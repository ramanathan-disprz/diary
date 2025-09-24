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

namespace backend.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<AuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockAuthService = new Mock<AuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();

            _controller = new AuthController(
                _mockMapper.Object,
                _mockAuthService.Object,
                _mockLogger.Object
            );
        }

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
        public void Create_WithInvalidRequest_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new UserRequest
            {
                // Missing required fields
            };

            // Add model validation errors
            _controller.ModelState.AddModelError("Name", "Name is required");
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
        }

        [Fact]
        public void Create_WithExistingEmail_ShouldReturnConflict()
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

            // Act
            var result = _controller.Create(request);

            // Assert
            // The exception will be caught by the global exception handler middleware
            // and converted to an appropriate HTTP response
            var exception = Assert.Throws<ConflictException>(() => result.Result);
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
        public void Create_WithNullRequest_ShouldReturnBadRequest()
        {
            // Arrange
            UserRequest request = null;

            // Act
            var result = _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
        }

        #endregion

        #region Login Tests

        [Fact]
        public void Login_WithValidCredentials_ShouldReturnOkWithToken()
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
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            var returnValue = Assert.IsType<AuthResponseDto>(okResult.Value);
            Assert.Equal(authResponse.Token, returnValue.Token);
            Assert.Equal(authResponse.ExpiresAt, returnValue.ExpiresAt);

            _mockAuthService.Verify(service => service.Login(request), Times.Once);
            VerifyLogging("POST " + URLConstants.Auth + "/login");
        }

        [Fact]
        public void Login_WithInvalidCredentials_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = "wrong_password"
            };

            _mockAuthService.Setup(service => service.Login(request))
                .Throws(new InvalidCredentialsException("Invalid credentials"));

            // Act
            var result = _controller.Login(request);

            // Assert
            // The exception will be caught by the global exception handler middleware
            // and converted to an appropriate HTTP response
            var exception = Assert.Throws<InvalidCredentialsException>(() => result.Result);
            Assert.Equal("Invalid credentials", exception.Message);

            _mockAuthService.Verify(service => service.Login(request), Times.Once);
            VerifyLogging("POST " + URLConstants.Auth + "/login");
        }

        [Fact]
        public void Login_WithNonExistentEmail_ShouldReturnNotFound()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "nonexistent@example.com",
                Password = "password123"
            };

            _mockAuthService.Setup(service => service.Login(request))
                .Throws(new EntityNotFoundException($"User with email {request.Email} not found"));

            // Act
            var result = _controller.Login(request);

            // Assert
            // The exception will be caught by the global exception handler middleware
            // and converted to an appropriate HTTP response
            var exception = Assert.Throws<EntityNotFoundException>(() => result.Result);
            Assert.Equal($"User with email {request.Email} not found", exception.Message);

            _mockAuthService.Verify(service => service.Login(request), Times.Once);
            VerifyLogging("POST " + URLConstants.Auth + "/login");
        }

        [Fact]
        public void Login_WithInvalidModelState_ShouldReturnBadRequest()
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

            // Act
            var result = _controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void Create_WithInvalidModelState_ShouldReturnBadRequest()
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

            // Act
            var result = _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
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
        public void Login_WithNullRequest_ShouldReturnBadRequest()
        {
            // Arrange
            LoginRequest request = null;

            // Act
            var result = _controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Create_WithEmptyEmail_ShouldReturnBadRequest()
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

            // Act
            var result = _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
        }

        [Fact]
        public void Create_WithEmptyPassword_ShouldReturnBadRequest()
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

            // Act
            var result = _controller.Create(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Register(It.IsAny<UserRequest>()), Times.Never);
        }

        [Fact]
        public void Login_WithEmptyEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "",
                Password = "password123"
            };

            // Add model validation errors
            _controller.ModelState.AddModelError("Email", "Email cannot be empty");

            // Act
            var result = _controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
        }

        [Fact]
        public void Login_WithEmptyPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@example.com",
                Password = ""
            };

            // Add model validation errors
            _controller.ModelState.AddModelError("Password", "Password cannot be empty");

            // Act
            var result = _controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(400, badRequestResult.StatusCode);

            _mockAuthService.Verify(service => service.Login(It.IsAny<LoginRequest>()), Times.Never);
        }

        #endregion

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
    }
}