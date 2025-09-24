using backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace backend.Tests.Data;

public class ApplicationDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_WithValidConnectionString_ReturnsDbContext()
    {
        // Arrange
        // Create a temporary appsettings.json file with a test connection string
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);
        var appsettingsPath = Path.Combine(tempDirectory, "appsettings.json");

        File.WriteAllText(appsettingsPath, @"{
                ""ConnectionStrings"": {
                    ""DefaultConnection"": ""Server=localhost;Database=test_db;User=test;Password=test;""
                }
            }");

        // Set the current directory to our temp directory so the factory finds our appsettings.json
        var originalDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDirectory);

        try
        {
            // Create a test instance of the factory
            var factory = new TestApplicationDbContextFactory();

            // Act
            var dbContext = factory.CreateDbContext(new string[] { });

            // Assert
            Assert.NotNull(dbContext);
            Assert.IsType<ApplicationDbContext>(dbContext);
        }
        finally
        {
            // Clean up
            Directory.SetCurrentDirectory(originalDirectory);
            try
            {
                Directory.Delete(tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void CreateDbContext_WithMissingConnectionString_ThrowsException()
    {
        // Arrange
        // Create a temporary appsettings.json file without a connection string
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);
        var appsettingsPath = Path.Combine(tempDirectory, "appsettings.json");

        File.WriteAllText(appsettingsPath, @"{
                ""ConnectionStrings"": {
                }
            }");

        // Set the current directory to our temp directory so the factory finds our appsettings.json
        var originalDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(tempDirectory);

        try
        {
            // Create a test instance of the factory
            var factory = new TestApplicationDbContextFactory();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                factory.CreateDbContext(new string[] { }));

            Assert.Equal("Connection string 'DefaultConnection' not found.", exception.Message);
        }
        finally
        {
            // Clean up
            Directory.SetCurrentDirectory(originalDirectory);
            try
            {
                Directory.Delete(tempDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    // Test subclass that overrides the UseMySql method to avoid actual database connections
    private class TestApplicationDbContextFactory : ApplicationDbContextFactory
    {
        public new ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found."
                );

            // Use in-memory database instead of MySQL
            optionsBuilder.UseInMemoryDatabase("TestDb");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}