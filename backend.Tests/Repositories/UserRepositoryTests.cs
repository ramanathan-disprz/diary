using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.Repository;
using backend.Exceptions;

namespace backend.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public UserRepositoryTests()
        {
            // Unique DB name per test run to isolate state
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public void Create_Should_Add_User_To_Database()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com", Password = "12345" };

            var result = repository.Create(user);

            result.Should().NotBeNull();
            context.Users.Count().Should().Be(1);
            context.Users.First().Name.Should().Be("Alice");
        }

        [Fact]
        public void FindById_WhenUserExists_ShouldReturnUser()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            var user = new User { Id = 1, Name = "Bob", Email = "bob@example.com", Password = "1234" };
            repository.Create(user);

            var result = repository.FindById(1);

            result.Should().NotBeNull();
            result!.Name.Should().Be("Bob");
        }

        [Fact]
        public void FindById_WhenUserDoesNotExist_ShouldReturnNull()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            var result = repository.FindById(999);

            result.Should().BeNull();
        }

        [Fact]
        public void FindOrThrow_WhenUserDoesNotExist_ShouldThrowException()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            Action act = () => repository.FindOrThrow(999);

            act.Should().Throw<EntityNotFoundException>()
                .WithMessage("Entity with id 999 not found");
        }

        [Fact]
        public void Update_Should_PersistChanges()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            var user = new User { Id = 1, Name = "Charlie", Email = "charlie@example.com", Password = "1234" };
            repository.Create(user);

            user.Name = "Updated Charlie";
            repository.Update(user);

            context.Users.First().Name.Should().Be("Updated Charlie");
        }

        [Fact]
        public void Delete_Should_RemoveUserFromDatabase()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            var user = new User { Id = 1, Name = "David", Email = "david@example.com", Password = "1234" };
            repository.Create(user);

            repository.Delete(user);

            context.Users.Any().Should().BeFalse();
        }

        [Fact]
        public void ExistsByEmail_WhenUserExists_ShouldReturnTrue()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            var user = new User { Id = 1, Name = "Eve", Email = "eve@example.com", Password = "1234" };
            repository.Create(user);

            var result = repository.ExistsByEmail("eve@example.com");

            result.Should().BeTrue();
        }

        [Fact]
        public void FindByEmail_WhenUserDoesNotExist_ShouldReturnNull()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            var result = repository.FindByEmail("notfound@example.com");

            result.Should().BeNull();
        }

        [Fact]
        public void FindByEmailOrThrow_WhenUserDoesNotExist_ShouldThrowException()
        {
            using var context = CreateContext();
            var repository = new UserRepository(context);

            Action act = () => repository.FindByEmailOrThrow("ghost@example.com");

            act.Should().Throw<EntityNotFoundException>()
                .WithMessage("User with email ghost@example.com not found");
        }
    }
}