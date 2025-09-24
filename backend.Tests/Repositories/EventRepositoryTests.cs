using backend.Data;
using backend.Exceptions;
using backend.Models;
using backend.Repository;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Repositories
{
    public class EventRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EventRepository _repository;

        public EventRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new EventRepository(_context);

            SeedData();
        }

        private void SeedData()
        {
            _context.Events.AddRange(
                new Event
                {
                    Id = 1,
                    UserId = 100,
                    Title = "Event 1",
                    StartDate = new DateOnly(2025, 9, 23),
                    EndDate = new DateOnly(2025, 9, 23),
                    StartTime = new TimeOnly(10, 0),
                    EndTime = new TimeOnly(11, 0)
                },
                new Event
                {
                    Id = 2,
                    UserId = 100,
                    Title = "Event 2",
                    StartDate = new DateOnly(2025, 9, 24),
                    EndDate = new DateOnly(2025, 9, 24),
                    StartTime = new TimeOnly(14, 0),
                    EndTime = new TimeOnly(15, 0)
                },
                new Event
                {
                    Id = 3,
                    UserId = 200,
                    Title = "Event 3",
                    StartDate = new DateOnly(2025, 9, 23),
                    EndDate = new DateOnly(2025, 9, 23),
                    StartTime = new TimeOnly(9, 0),
                    EndTime = new TimeOnly(10, 0)
                }
            );

            _context.SaveChanges();
        }

        [Fact]
        public void FindAllByUserIdAndDate_ShouldReturnEventsForGivenUserAndDate()
        {
            var result = _repository.FindAllByUserIdAndDate(100, new DateOnly(2025, 9, 23));

            Assert.Single(result);
            Assert.Equal("Event 1", result.First().Title);
        }

        [Fact]
        public void FindAllByUserIdAndDate_NoEvents_ShouldReturnEmpty()
        {
            var result = _repository.FindAllByUserIdAndDate(100, new DateOnly(2025, 9, 25));
            Assert.Empty(result);
        }

        [Fact]
        public void FindAllByUserIdAndRange_ShouldReturnEventsWithinRange()
        {
            var result = _repository.FindAllByUserIdAndRange(100, new DateOnly(2025, 9, 23), new DateOnly(2025, 9, 24));

            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Title == "Event 1");
            Assert.Contains(result, e => e.Title == "Event 2");
        }

        [Fact]
        public void FindByUserIdAndId_ShouldReturnCorrectEvent()
        {
            var result = _repository.FindByUserIdAndId(100, 1);

            Assert.NotNull(result);
            Assert.Equal("Event 1", result.Title);
        }

        [Fact]
        public void FindByUserIdAndId_ShouldReturnNullIfNotFound()
        {
            var result = _repository.FindByUserIdAndId(100, 999);
            Assert.Null(result);
        }

        [Fact]
        public void FindByUserIdAndIdOrThrow_ShouldReturnEvent()
        {
            var result = _repository.FindByUserIdAndIdOrThrow(100, 1);
            Assert.Equal("Event 1", result.Title);
        }

        [Fact]
        public void FindByUserIdAndIdOrThrow_ShouldThrowIfNotFound()
        {
            Assert.Throws<EntityNotFoundException>(() =>
                _repository.FindByUserIdAndIdOrThrow(100, 999));
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}