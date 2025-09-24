using backend.Exceptions;
using backend.Models;
using backend.Utils;
using Xunit;

namespace backend.Tests.Utils
{
    public class EventValidatorTests
    {
        [Fact]
        public void ValidateEvent_WithValidEvent_ShouldNotThrowException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            // Act & Assert
            // No exception should be thrown
            EventValidator.ValidateEvent(eventItem);
        }

        [Fact]
        public void ValidateEvent_WithEndTimeBeforeStartTime_ShouldThrowBadRequestException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(9, 0), // End time before start time
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            // Act & Assert
            var exception = Assert.Throws<BadRequestException>(() =>
                EventValidator.ValidateEvent(eventItem));

            Assert.Equal("End Time must be greater than Start Time.", exception.Message);
        }

        [Fact]
        public void ValidateEvent_WithDateBeforeYear1900_ShouldThrowBadRequestException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(1899, 12, 31), // Date before 1900
                EndDate = new DateOnly(1899, 12, 31),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            // Act & Assert
            var exception = Assert.Throws<BadRequestException>(() =>
                EventValidator.ValidateEvent(eventItem));

            Assert.Equal("Event Date cannot be earlier than year 1900.", exception.Message);
        }

        [Fact]
        public void EnsureNoConflict_WithNoConflicts_ShouldNotThrowException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            var existingEvents = new List<Event>
            {
                new Event
                {
                    Id = 2,
                    UserId = 1,
                    Title = "Existing Event 1",
                    StartDate = new DateOnly(2023, 10, 15),
                    EndDate = new DateOnly(2023, 10, 15),
                    StartTime = new TimeOnly(11, 0), // After the test event
                    EndTime = new TimeOnly(12, 0),
                    TimeZone = "Asia/Kolkata",
                    EventType = "Work"
                },
                new Event
                {
                    Id = 3,
                    UserId = 1,
                    Title = "Existing Event 2",
                    StartDate = new DateOnly(2023, 10, 15),
                    EndDate = new DateOnly(2023, 10, 15),
                    StartTime = new TimeOnly(7, 0), // Before the test event
                    EndTime = new TimeOnly(8, 0),
                    TimeZone = "Asia/Kolkata",
                    EventType = "Work"
                }
            };

            // Act & Assert
            // No exception should be thrown
            EventValidator.EnsureNoConflict(eventItem, existingEvents);
        }

        [Fact]
        public void EnsureNoConflict_WithConflictingEvent_ShouldThrowConflictException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            var existingEvents = new List<Event>
            {
                new Event
                {
                    Id = 2,
                    UserId = 1,
                    Title = "Conflicting Event",
                    StartDate = new DateOnly(2023, 10, 15),
                    EndDate = new DateOnly(2023, 10, 15),
                    StartTime = new TimeOnly(9, 30), // Overlaps with test event
                    EndTime = new TimeOnly(10, 30),
                    TimeZone = "Asia/Kolkata",
                    EventType = "Work"
                }
            };

            // Act & Assert
            var exception = Assert.Throws<ConflictException>(() =>
                EventValidator.EnsureNoConflict(eventItem, existingEvents));

            Assert.Equal("Event scheduling conflicts with an existing event", exception.Message);
        }

        [Fact]
        public void EnsureNoConflict_WithSameEventId_ShouldNotThrowException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            var existingEvents = new List<Event>
            {
                new Event
                {
                    Id = 1, // Same ID as test event
                    UserId = 1,
                    Title = "Same Event",
                    StartDate = new DateOnly(2023, 10, 15),
                    EndDate = new DateOnly(2023, 10, 15),
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0),
                    TimeZone = "Asia/Kolkata",
                    EventType = "Work"
                }
            };

            // Act & Assert
            // No exception should be thrown because the events have the same ID
            EventValidator.EnsureNoConflict(eventItem, existingEvents);
        }

        [Fact]
        public void EnsureNoConflict_WithEventStartingAtEndOfAnother_ShouldNotThrowException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(10, 0), // Starts exactly when the other ends
                EndTime = new TimeOnly(11, 0),
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            var existingEvents = new List<Event>
            {
                new Event
                {
                    Id = 2,
                    UserId = 1,
                    Title = "Existing Event",
                    StartDate = new DateOnly(2023, 10, 15),
                    EndDate = new DateOnly(2023, 10, 15),
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0), // Ends exactly when the test event starts
                    TimeZone = "Asia/Kolkata",
                    EventType = "Work"
                }
            };

            // Act & Assert
            // No exception should be thrown because the events don't overlap
            EventValidator.EnsureNoConflict(eventItem, existingEvents);
        }

        [Fact]
        public void EnsureNoConflict_WithEventEndingAtStartOfAnother_ShouldNotThrowException()
        {
            // Arrange
            var eventItem = new Event
            {
                Id = 1,
                UserId = 1,
                Title = "Test Event",
                StartDate = new DateOnly(2023, 10, 15),
                EndDate = new DateOnly(2023, 10, 15),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0), // Ends exactly when the other starts
                TimeZone = "Asia/Kolkata",
                EventType = "Work"
            };

            var existingEvents = new List<Event>
            {
                new Event
                {
                    Id = 2,
                    UserId = 1,
                    Title = "Existing Event",
                    StartDate = new DateOnly(2023, 10, 15),
                    EndDate = new DateOnly(2023, 10, 15),
                    StartTime = new TimeOnly(10, 0), // Starts exactly when the test event ends
                    EndTime = new TimeOnly(11, 0),
                    TimeZone = "Asia/Kolkata",
                    EventType = "Work"
                }
            };

            // Act & Assert
            // No exception should be thrown because the events don't overlap
            EventValidator.EnsureNoConflict(eventItem, existingEvents);
        }
    }
}