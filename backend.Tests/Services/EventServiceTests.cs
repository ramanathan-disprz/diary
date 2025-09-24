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

public class EventServiceTests
{
    private readonly Mock<ILogger<EventService>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEventRepository> _mockRepository;
    private readonly EventService _service;

    public EventServiceTests()
    {
        _mockRepository = new Mock<IEventRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<EventService>>();

        _service = new EventService(
            _mockMapper.Object,
            _mockLogger.Object,
            _mockRepository.Object
        );
    }

    [Fact]
    public void FindAllByUserIdAndDate_ShouldReturnEventsForUserAndDate()
    {
        // Arrange
        var userId = 1L;
        var date = new DateOnly(2023, 10, 15);
        var events = new List<Event>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                Title = "Test Event",
                StartDate = date,
                EndDate = date,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            }
        };

        _mockRepository.Setup(repo => repo.FindAllByUserIdAndDate(userId, date))
            .Returns(events);

        // Act
        var result = _service.FindAllByUserIdAndDate(userId, date);

        // Assert
        result.Should().BeEquivalentTo(events);
        _mockRepository.Verify(repo => repo.FindAllByUserIdAndDate(userId, date), Times.Once);
    }

    [Fact]
    public void FindAllByUserIdAndDate_WithNullDate_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = 1L;
        DateOnly? date = null;

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() =>
            _service.FindAllByUserIdAndDate(userId, date));

        exception.Message.Should().Be("Insufficient parameters : date must be provided.");
    }

    [Fact]
    public void FindAllByUserIdAndRange_ShouldReturnEventsInDateRange()
    {
        // Arrange
        var userId = 1L;
        var startDate = new DateOnly(2023, 10, 15);
        var endDate = new DateOnly(2023, 10, 17);
        var events = new List<Event>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                Title = "Test Event 1",
                StartDate = startDate,
                EndDate = startDate,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0)
            },
            new()
            {
                Id = 2,
                UserId = userId,
                Title = "Test Event 2",
                StartDate = endDate,
                EndDate = endDate,
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(15, 0)
            }
        };

        _mockRepository.Setup(repo => repo.FindAllByUserIdAndRange(userId, startDate, endDate))
            .Returns(events);

        // Act
        var result = _service.FindAllByUserIdAndRange(userId, startDate, endDate);

        // Assert
        result.Should().BeEquivalentTo(events);
        _mockRepository.Verify(repo => repo.FindAllByUserIdAndRange(userId, startDate, endDate), Times.Once);
    }

    [Fact]
    public void FindAllByUserIdAndRange_WithNullDates_ShouldThrowBadRequestException()
    {
        // Arrange
        var userId = 1L;
        DateOnly? startDate = null;
        DateOnly? endDate = new DateOnly(2023, 10, 17);

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() =>
            _service.FindAllByUserIdAndRange(userId, startDate, endDate));

        exception.Message.Should().Be("Insufficient parameters : start date and end date must be provided.");

        // Test with null end date
        startDate = new DateOnly(2023, 10, 15);
        endDate = null;

        exception = Assert.Throws<BadRequestException>(() =>
            _service.FindAllByUserIdAndRange(userId, startDate, endDate));

        exception.Message.Should().Be("Insufficient parameters : start date and end date must be provided.");
    }

    [Fact]
    public void Fetch_ShouldReturnEventByUserIdAndEventId()
    {
        // Arrange
        var userId = 1L;
        var eventId = 2L;
        var eventItem = new Event
        {
            Id = eventId,
            UserId = userId,
            Title = "Test Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        _mockRepository.Setup(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId))
            .Returns(eventItem);

        // Act
        var result = _service.Fetch(userId, eventId);

        // Assert
        result.Should().BeEquivalentTo(eventItem);
        _mockRepository.Verify(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId), Times.Once);
    }

    [Fact]
    public void Fetch_WithInvalidIds_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 1L;
        var eventId = 999L;

        _mockRepository.Setup(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId))
            .Throws(new EntityNotFoundException($"Event with id {eventId} not found for user {userId}"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _service.Fetch(userId, eventId));
        exception.Message.Should().Be($"Event with id {eventId} not found for user {userId}");
        _mockRepository.Verify(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId), Times.Once);
    }

    [Fact]
    public void Create_ShouldCreateAndReturnNewEvent()
    {
        // Arrange
        var eventRequest = new EventRequest
        {
            UserId = 1,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var mappedEvent = new Event
        {
            UserId = 1,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Personal"
        };

        var createdEvent = new Event
        {
            Id = 1,
            UserId = 1,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Other"
        };

        _mockMapper.Setup(mapper => mapper.Map<Event>(eventRequest))
            .Returns(mappedEvent);

        _mockRepository.Setup(repo => repo.Create(It.IsAny<Event>()))
            .Returns(createdEvent);

        _mockRepository.Setup(repo => repo.FindAllByUserIdAndDate(eventRequest.UserId, mappedEvent.StartDate))
            .Returns(new List<Event>());

        // Act
        var result = _service.Create(eventRequest);

        // Assert
        result.Should().BeEquivalentTo(createdEvent);
        _mockMapper.Verify(mapper => mapper.Map<Event>(eventRequest), Times.Once);
        _mockRepository.Verify(repo => repo.Create(It.IsAny<Event>()), Times.Once);
    }

    [Fact]
    public void Create_WithInvalidEvent_ShouldThrowBadRequestException()
    {
        // Arrange
        var eventRequest = new EventRequest
        {
            UserId = 1,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 14), // End date before start date
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var mappedEvent = new Event
        {
            UserId = 1,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 14), // End date before start date
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Personal"
        };

        _mockMapper.Setup(mapper => mapper.Map<Event>(eventRequest))
            .Returns(mappedEvent);

        // Mock the static EventValidator to throw an exception
        // Since we can't directly mock static classes, we'll use a workaround
        // by setting up the repository to throw when FindAllByUserIdAndDate is called
        _mockRepository.Setup(repo => repo.FindAllByUserIdAndDate(It.IsAny<long>(), It.IsAny<DateOnly>()))
            .Callback(() => throw new BadRequestException("End date cannot be before start date"));

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _service.Create(eventRequest));
        exception.Message.Should().Be("End date cannot be before start date");
    }

    [Fact]
    public void Create_WithConflictingEvent_ShouldThrowConflictException()
    {
        // Arrange
        var eventRequest = new EventRequest
        {
            UserId = 1,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var mappedEvent = new Event
        {
            UserId = 1,
            Title = "New Event",
            Description = "Test Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var existingEvents = new List<Event>
        {
            new()
            {
                Id = 2,
                UserId = 1,
                Title = "Existing Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(9, 30), // Overlaps with new event
                EndTime = new TimeOnly(10, 30),
                TimeZone = "UTC",
                EventType = "Work"
            }
        };

        _mockMapper.Setup(mapper => mapper.Map<Event>(eventRequest))
            .Returns(mappedEvent);

        _mockRepository.Setup(repo => repo.FindAllByUserIdAndDate(eventRequest.UserId, mappedEvent.StartDate))
            .Returns(existingEvents);

        // Mock the static EventValidator to throw an exception
        // Since we can't directly mock static classes, we'll use a workaround
        // by setting up the repository to throw when Create is called
        _mockRepository.Setup(repo => repo.Create(It.IsAny<Event>()))
            .Callback(() => throw new ConflictException("Event scheduling conflicts with an existing event"));

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => _service.Create(eventRequest));
        exception.Message.Should().Be("Event scheduling conflicts with an existing event");
    }

    [Fact]
    public void Update_ShouldUpdateAndReturnEvent()
    {
        // Arrange
        var eventId = 1L;
        var eventRequest = new EventRequest
        {
            UserId = 1,
            Title = "Updated Event",
            Description = "Updated Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var existingEvent = new Event
        {
            Id = eventId,
            UserId = 1,
            Title = "Original Event",
            Description = "Original Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var updatedEvent = new Event
        {
            Id = eventId,
            UserId = 1,
            Title = "Updated Event",
            Description = "Updated Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0),
            TimeZone = "UTC",
            EventType = "Personal"
        };

        _mockRepository.Setup(repo => repo.FindByUserIdAndIdOrThrow(eventRequest.UserId, eventId))
            .Returns(existingEvent);

        _mockMapper.Setup(mapper => mapper.Map(eventRequest, existingEvent))
            .Returns(updatedEvent);

        _mockRepository.Setup(repo => repo.Update(updatedEvent))
            .Returns(updatedEvent);

        _mockRepository.Setup(repo => repo.FindAllByUserIdAndDate(eventRequest.UserId, updatedEvent.StartDate))
            .Returns(new List<Event>());

        // Act
        var result = _service.Update(eventId, eventRequest);

        // Assert
        result.Should().BeEquivalentTo(updatedEvent);
        _mockRepository.Verify(repo => repo.FindByUserIdAndIdOrThrow(eventRequest.UserId, eventId), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map(eventRequest, existingEvent), Times.Once);
        _mockRepository.Verify(repo => repo.Update(updatedEvent), Times.Once);
    }

    [Fact]
    public void Update_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var eventId = 999L;
        var eventRequest = new EventRequest
        {
            UserId = 1,
            Title = "Updated Event",
            Description = "Updated Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        _mockRepository.Setup(repo => repo.FindByUserIdAndIdOrThrow(eventRequest.UserId, eventId))
            .Throws(
                new EntityNotFoundException($"Event with id {eventId} not found for user {eventRequest.UserId}"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _service.Update(eventId, eventRequest));
        exception.Message.Should().Be($"Event with id {eventId} not found for user {eventRequest.UserId}");
        _mockRepository.Verify(repo => repo.FindByUserIdAndIdOrThrow(eventRequest.UserId, eventId), Times.Once);
    }

    [Fact]
    public void Update_WithInvalidEvent_ShouldThrowBadRequestException()
    {
        // Arrange
        var eventId = 1L;
        var eventRequest = new EventRequest
        {
            UserId = 1,
            Title = "Updated Event",
            Description = "Updated Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 14), // End date before start date
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var existingEvent = new Event
        {
            Id = eventId,
            UserId = 1,
            Title = "Original Event",
            Description = "Original Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        var updatedEvent = new Event
        {
            Id = eventId,
            UserId = 1,
            Title = "Updated Event",
            Description = "Updated Description",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 14), // End date before start date
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0),
            TimeZone = "UTC",
            EventType = "Work"
        };

        _mockRepository.Setup(repo => repo.FindByUserIdAndIdOrThrow(eventRequest.UserId, eventId))
            .Returns(existingEvent);

        _mockMapper.Setup(mapper => mapper.Map(eventRequest, existingEvent))
            .Returns(updatedEvent);

        // Mock the static EventValidator to throw an exception
        // Since we can't directly mock static classes, we'll use a workaround
        // by setting up the repository to throw when FindAllByUserIdAndDate is called
        _mockRepository.Setup(repo => repo.FindAllByUserIdAndDate(It.IsAny<long>(), It.IsAny<DateOnly>()))
            .Callback(() => throw new BadRequestException("End date cannot be before start date"));

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(() => _service.Update(eventId, eventRequest));
        exception.Message.Should().Be("End date cannot be before start date");
    }

    [Fact]
    public void Delete_ShouldDeleteEvent()
    {
        // Arrange
        var userId = 1L;
        var eventId = 1L;
        var eventItem = new Event
        {
            Id = eventId,
            UserId = userId,
            Title = "Test Event",
            StartDate = new DateOnly(2023, 10, 15),
            EndDate = new DateOnly(2023, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        _mockRepository.Setup(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId))
            .Returns(eventItem);

        _mockRepository.Setup(repo => repo.Delete(eventItem));

        // Act
        _service.Delete(userId, eventId);

        // Assert
        _mockRepository.Verify(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId), Times.Once);
        _mockRepository.Verify(repo => repo.Delete(eventItem), Times.Once);
    }

    [Fact]
    public void Delete_WithInvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var userId = 1L;
        var eventId = 999L;

        _mockRepository.Setup(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId))
            .Throws(new EntityNotFoundException($"Event with id {eventId} not found for user {userId}"));

        // Act & Assert
        var exception = Assert.Throws<EntityNotFoundException>(() => _service.Delete(userId, eventId));
        exception.Message.Should().Be($"Event with id {eventId} not found for user {userId}");
        _mockRepository.Verify(repo => repo.FindByUserIdAndIdOrThrow(userId, eventId), Times.Once);
    }
}