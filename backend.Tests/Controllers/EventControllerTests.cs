using AutoMapper;
using backend.Controllers;
using backend.Dtos;
using backend.Exceptions;
using backend.Models;
using backend.Requests;
using backend.Security.Constants;
using backend.Service;
using backend.Utils.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.Tests.Controllers;

public class EventControllerTests
{
    private readonly EventController _controller;
    private readonly Mock<IEventService> _mockEventService;
    private readonly Mock<ILogger<EventController>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly long _userId = 1L;

    public EventControllerTests()
    {
        _mockMapper = new Mock<IMapper>();
        _mockEventService = new Mock<IEventService>();
        _mockLogger = new Mock<ILogger<EventController>>();

        // Setup controller with HttpContext
        _controller = new EventController(
            _mockMapper.Object,
            _mockLogger.Object,
            _mockEventService.Object
        );

        // Setup HttpContext with authenticated user
        var httpContext = new DefaultHttpContext();
        httpContext.Items = new Dictionary<object, object>();
        httpContext.Items[HttpContextItemKeys.UserId] = _userId;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    #region GetAllEvents Tests

    [Fact]
    public void GetAllEvents_WithDate_ShouldReturnEventsForDate()
    {
        // Arrange
        var date = new DateOnly(2023, 10, 15);
        var events = new List<Event>
        {
            new()
            {
                Id = 1,
                UserId = _userId,
                Title = "Test Event",
                StartDate = date,
                EndDate = date,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            }
        };

        var eventDtos = new List<EventDto>
        {
            new()
            {
                Id = 1,
                UserId = _userId,
                Title = "Test Event",
                StartDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o"),
                EndDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o")
            }
        };

        _mockEventService.Setup(service => service.FindAllByUserIdAndDate(_userId, date)).Returns(events);
        _mockMapper.Setup(mapper => mapper.Map<IEnumerable<EventDto>>(events)).Returns(eventDtos);

        // Act
        var result = _controller.GetAllEvents(date, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<EventDto>>(okResult.Value);
        Assert.Single(returnValue);

        _mockEventService.Verify(service => service.FindAllByUserIdAndDate(_userId, date), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<IEnumerable<EventDto>>(events), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("GET") && v.ToString().Contains("date=")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetAllEvents_WithDateRange_ShouldReturnEventsInRange()
    {
        // Arrange
        var startDate = new DateOnly(2023, 10, 15);
        var endDate = new DateOnly(2023, 10, 17);
        var events = new List<Event>
        {
            new()
            {
                Id = 1,
                UserId = _userId,
                Title = "Test Event 1",
                StartDate = startDate,
                EndDate = startDate,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            },
            new()
            {
                Id = 2,
                UserId = _userId,
                Title = "Test Event 2",
                StartDate = endDate,
                EndDate = endDate,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(15, 0)
            }
        };

        var eventDtos = new List<EventDto>
        {
            new()
            {
                Id = 1,
                UserId = _userId,
                Title = "Test Event 1",
                StartDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o"),
                EndDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o")
            },
            new()
            {
                Id = 2,
                UserId = _userId,
                Title = "Test Event 2",
                StartDateTime = new DateTime(2023, 10, 17, 14, 0, 0).ToString("o"),
                EndDateTime = new DateTime(2023, 10, 17, 15, 0, 0).ToString("o")
            }
        };

        _mockEventService.Setup(service => service.FindAllByUserIdAndRange(_userId, startDate, endDate))
            .Returns(events);
        _mockMapper.Setup(mapper => mapper.Map<IEnumerable<EventDto>>(events)).Returns(eventDtos);

        // Act
        var result = _controller.GetAllEvents(null, startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsAssignableFrom<IEnumerable<EventDto>>(okResult.Value);
        Assert.Equal(2, returnValue.Count());

        _mockEventService.Verify(service => service.FindAllByUserIdAndRange(_userId, startDate, endDate),
            Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<IEnumerable<EventDto>>(events), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains("GET") && v.ToString().Contains("start=") &&
                    v.ToString().Contains("end=")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetAllEvents_WithNoParameters_ShouldThrowBadRequestException()
    {
        // Arrange - no parameters

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.GetAllEvents(null, null, null));
        Assert.Equal("Insufficient parameters : date or start and end date must be provided.", exception.Message);

        _mockEventService.Verify(service => service.FindAllByUserIdAndDate(It.IsAny<long>(), It.IsAny<DateOnly>()),
            Times.Never);
        _mockEventService.Verify(
            service => service.FindAllByUserIdAndRange(It.IsAny<long>(), It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public void GetAllEvents_WithStartDateOnly_ShouldThrowBadRequestException()
    {
        // Arrange
        var startDate = new DateOnly(2023, 10, 15);

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.GetAllEvents(null, startDate, null));
        Assert.Equal("Insufficient parameters : date or start and end date must be provided.", exception.Message);

        _mockEventService.Verify(service => service.FindAllByUserIdAndDate(It.IsAny<long>(), It.IsAny<DateOnly>()),
            Times.Never);
        _mockEventService.Verify(
            service => service.FindAllByUserIdAndRange(It.IsAny<long>(), It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public void GetAllEvents_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var date = new DateOnly(2023, 10, 15);

        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception =
            Assert.Throws<InvalidCredentialsException>(() => _controller.GetAllEvents(date, null, null));
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockEventService.Verify(service => service.FindAllByUserIdAndDate(It.IsAny<long>(), It.IsAny<DateOnly>()),
            Times.Never);
    }

    #endregion

    #region Fetch Tests

    [Fact]
    public void Fetch_ShouldReturnEvent()
    {
        // Arrange
        var eventId = 1L;
        var eventItem = new Event
        {
            Id = eventId,
            UserId = _userId,
            Title = "Test Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        var eventDto = new EventDto
        {
            Id = eventId,
            UserId = _userId,
            Title = "Test Event",
            StartDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o"),
            EndDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o")
        };

        _mockEventService.Setup(service => service.Fetch(_userId, eventId)).Returns(eventItem);
        _mockMapper.Setup(mapper => mapper.Map<EventDto>(eventItem)).Returns(eventDto);

        // Act
        var result = _controller.Fetch(eventId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<EventDto>(okResult.Value);
        Assert.Equal(eventDto.Id, returnValue.Id);
        Assert.Equal(eventDto.Title, returnValue.Title);

        _mockEventService.Verify(service => service.Fetch(_userId, eventId), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<EventDto>(eventItem), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"GET {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Fetch_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var eventId = 1L;

        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Fetch(eventId));
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockEventService.Verify(service => service.Fetch(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public void Fetch_WithNonExistentEvent_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var eventId = 999L;
        _mockEventService.Setup(service => service.Fetch(_userId, eventId))
            .Throws(new EntityNotFoundException($"Event with id {eventId} not found for user {_userId}"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _controller.Fetch(eventId));
        Assert.Equal($"Event with id {eventId} not found for user {_userId}", exception.Message);

        _mockEventService.Verify(service => service.Fetch(_userId, eventId), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"GET {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_ShouldCreateAndReturnEvent()
    {
        // Arrange
        var request = new EventRequest
        {
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var createdEvent = new Event
        {
            Id = 1,
            UserId = _userId,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var eventDto = new EventDto
        {
            Id = 1,
            UserId = _userId,
            Title = "New Event",
            Description = "Test Description",
            StartDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o"),
            EndDateTime = new DateTime(2023, 10, 15, 9, 0, 0).ToString("o"),
            TimeZone = "UTC",
            EventType = "Work"
        };

        _mockEventService.Setup(service => service.Create(It.Is<EventRequest>(r => r.UserId == _userId)))
            .Returns(createdEvent);
        _mockMapper.Setup(mapper => mapper.Map<EventDto>(createdEvent)).Returns(eventDto);

        // Act
        var result = _controller.Create(request);

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        var returnValue = Assert.IsType<EventDto>(createdResult.Value);
        Assert.Equal(eventDto.Id, returnValue.Id);
        Assert.Equal(eventDto.Title, returnValue.Title);

        // Verify that UserId was set correctly
        Assert.Equal(_userId, request.UserId);

        _mockEventService.Verify(service => service.Create(It.Is<EventRequest>(r => r.UserId == _userId)),
            Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<EventDto>(createdEvent), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"POST {URLConstants.Events}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Create_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new EventRequest
        {
            Title = "New Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Create(request));
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockEventService.Verify(service => service.Create(It.IsAny<EventRequest>()), Times.Never);
    }

    [Fact]
    public void Create_WithInvalidEvent_ShouldThrowBadRequestException()
    {
        // Arrange
        var request = new EventRequest
        {
            Title = "New Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(9, 0) // End time before start time
        };

        _mockEventService.Setup(service => service.Create(It.Is<EventRequest>(r => r.UserId == _userId)))
            .Throws(new BadRequestException("End Time must be greater than Start Time."));

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Create(request));
        Assert.Equal("End Time must be greater than Start Time.", exception.Message);

        // Verify that UserId was set correctly
        Assert.Equal(_userId, request.UserId);

        _mockEventService.Verify(service => service.Create(It.Is<EventRequest>(r => r.UserId == _userId)),
            Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"POST {URLConstants.Events}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Create_WithConflictingEvent_ShouldThrowConflictException()
    {
        // Arrange
        var request = new EventRequest
        {
            Title = "New Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        _mockEventService.Setup(service => service.Create(It.Is<EventRequest>(r => r.UserId == _userId)))
            .Throws(new ConflictException("Event scheduling conflicts with an existing event"));

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _controller.Create(request));
        Assert.Equal("Event scheduling conflicts with an existing event", exception.Message);

        // Verify that UserId was set correctly
        Assert.Equal(_userId, request.UserId);

        _mockEventService.Verify(service => service.Create(It.Is<EventRequest>(r => r.UserId == _userId)),
            Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"POST {URLConstants.Events}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_ShouldUpdateAndReturnEvent()
    {
        // Arrange
        var eventId = 1L;
        var request = new EventRequest
        {
            Title = "Updated Event",
            Description = "Updated Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var updatedEvent = new Event
        {
            Id = eventId,
            UserId = _userId,
            Title = "Updated Event",
            Description = "Updated Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var eventDto = new EventDto
        {
            Id = eventId,
            UserId = _userId,
            Title = "Updated Event",
            Description = "Updated Description",
            StartDateTime = new DateTime(2023, 10, 15, 11, 0, 0).ToString("o"),
            EndDateTime = new DateTime(2023, 10, 15, 12, 0, 0).ToString("o"),
            TimeZone = "UTC",
            EventType = "Work"
        };

        _mockEventService.Setup(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)))
            .Returns(updatedEvent);
        _mockMapper.Setup(mapper => mapper.Map<EventDto>(updatedEvent)).Returns(eventDto);

        // Act
        var result = _controller.Update(eventId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<EventDto>(okResult.Value);
        Assert.Equal(eventDto.Id, returnValue.Id);
        Assert.Equal(eventDto.Title, returnValue.Title);

        // Verify that UserId was set correctly
        Assert.Equal(_userId, request.UserId);

        _mockEventService.Verify(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)),
            Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<EventDto>(updatedEvent), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"PUT {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Update_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var eventId = 1L;
        var request = new EventRequest
        {
            Title = "Updated Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Update(eventId, request));
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockEventService.Verify(service => service.Update(It.IsAny<long>(), It.IsAny<EventRequest>()),
            Times.Never);
    }

    [Fact]
    public void Update_WithNonExistentEvent_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var eventId = 999L;
        var request = new EventRequest
        {
            Title = "Updated Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0)
        };

        _mockEventService.Setup(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)))
            .Throws(new EntityNotFoundException($"Event with id {eventId} not found for user {_userId}"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _controller.Update(eventId, request));
        Assert.Equal($"Event with id {eventId} not found for user {_userId}", exception.Message);

        // Verify that UserId was set correctly
        Assert.Equal(_userId, request.UserId);

        _mockEventService.Verify(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)),
            Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"PUT {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Update_WithInvalidEvent_ShouldThrowBadRequestException()
    {
        // Arrange
        var eventId = 1L;
        var request = new EventRequest
        {
            Title = "Updated Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(11, 0) // End time before start time
        };

        _mockEventService.Setup(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)))
            .Throws(new BadRequestException("End Time must be greater than Start Time."));

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _controller.Update(eventId, request));
        Assert.Equal("End Time must be greater than Start Time.", exception.Message);

        // Verify that UserId was set correctly
        Assert.Equal(_userId, request.UserId);

        _mockEventService.Verify(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)),
            Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"PUT {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Update_WithConflictingEvent_ShouldThrowConflictException()
    {
        // Arrange
        var eventId = 1L;
        var request = new EventRequest
        {
            Title = "Updated Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0)
        };

        _mockEventService.Setup(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)))
            .Throws(new ConflictException("Event scheduling conflicts with an existing event"));

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _controller.Update(eventId, request));
        Assert.Equal("Event scheduling conflicts with an existing event", exception.Message);

        // Verify that UserId was set correctly
        Assert.Equal(_userId, request.UserId);

        _mockEventService.Verify(service => service.Update(eventId, It.Is<EventRequest>(r => r.UserId == _userId)),
            Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"PUT {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldDeleteEventAndReturnNoContent()
    {
        // Arrange
        var eventId = 1L;
        _mockEventService.Setup(service => service.Delete(_userId, eventId));

        // Act
        var result = _controller.Delete(eventId);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _mockEventService.Verify(service => service.Delete(_userId, eventId), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"DELETE {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void Delete_WithNoAuthenticatedUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var eventId = 1L;

        // Clear the UserId from HttpContext
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() => _controller.Delete(eventId));
        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);

        _mockEventService.Verify(service => service.Delete(It.IsAny<long>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public void Delete_WithNonExistentEvent_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var eventId = 999L;
        _mockEventService.Setup(service => service.Delete(_userId, eventId))
            .Throws(new EntityNotFoundException($"Event with id {eventId} not found for user {_userId}"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _controller.Delete(eventId));
        Assert.Equal($"Event with id {eventId} not found for user {_userId}", exception.Message);

        _mockEventService.Verify(service => service.Delete(_userId, eventId), Times.Once);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString().Contains($"DELETE {URLConstants.Events}/{eventId}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region GetAuthenticatedUserId Tests

    [Fact]
    public void GetAuthenticatedUserId_WithValidUser_ShouldReturnUserId()
    {
        // Arrange - HttpContext already set up in constructor

        // Act - Call a method that uses GetAuthenticatedUserId
        var result = _controller.GetAllEvents(new DateOnly(2023, 10, 15), null, null);

        // Assert - Verify the service was called with the correct user ID
        _mockEventService.Verify(service => service.FindAllByUserIdAndDate(_userId, It.IsAny<DateOnly>()),
            Times.Once);
    }

    [Fact]
    public void GetAuthenticatedUserId_WithNoUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        _controller.ControllerContext.HttpContext.Items.Clear();

        // Act & Assert
        var exception = Assert.Throws<InvalidCredentialsException>(() =>
            _controller.GetAllEvents(new DateOnly(2023, 10, 15), null, null));

        Assert.Equal("Authentication failed: missing or invalid user ID in token", exception.Message);
    }

    #endregion
}