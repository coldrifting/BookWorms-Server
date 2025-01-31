using BookwormsServer;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace BookwormsServerTesting.Fixtures;

/// <summary>
/// Fixture responsible for managing database concerns.
/// <br/>
/// Handles bootstrapping the database, seeding test data on scope start, and
/// clearing the database on scope end. Also provides DbContext objects using
/// the provided config's connection string.
/// </summary>
public class DatabaseFixture : IDisposable
{
    private static readonly object Lock = new();
    private static bool _databaseInitialized;
    private string _connectionString = null!;

    public DatabaseFixture()
    {
        lock (Lock)
        {
            if (_databaseInitialized) return;

            var appConfig = CreateTestConfiguration();
            ConfigureDbForTests(appConfig);
            
            _databaseInitialized = true;
        }
    }

    public BookwormsDbContext CreateDbContext()
    {
        var dbContextOptions = new DbContextOptionsBuilder<BookwormsDbContext>()
            .UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString))
            .Options;
        var dbContext = new BookwormsDbContext(dbContextOptions);
        return dbContext;
    }

    public void Dispose()
    {
        using var dbContext = CreateDbContext();
        dbContext.Clear();
        GC.SuppressFinalize(this);
    }



    private static IConfigurationRoot CreateTestConfiguration()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{env}.json", true, true) // tests run in the Test environment
            .AddEnvironmentVariables()
            .Build();
    }
    
    private void ConfigureDbForTests(IConfigurationRoot appConfig)
    {
        _connectionString = appConfig.GetConnectionString("DefaultConnection") ?? "";
        
        // If the current environment settings say to use Testcontainers, create that container now
        bool useTestcontainers = appConfig.GetValue("Database:TestContainer:Use", false);
        if (useTestcontainers)
        {
            MySqlConnectionStringBuilder connString = new MySqlConnectionStringBuilder(_connectionString);

            // Create the Testcontainers database
            var container = new ContainerBuilder()
                .WithImage($"mysql:{appConfig.GetValue<string>("Database:Version")}")
                .WithName($"mysql-container-{Guid.NewGuid()}")
                .WithEnvironment("MYSQL_DATABASE", connString.Database)
                .WithEnvironment("MYSQL_ROOT_PASSWORD", connString.Password)
                .WithPortBinding(3306, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
                .Build();
			
            Task.Run(async () => await container.StartAsync()).Wait();
				
            // Override DB Context to use the TestContainers database
            _connectionString = _connectionString
                .Replace("localhost", container.Hostname)
                .Replace("3306", container.GetMappedPublicPort(3306).ToString());
        }

        
        // Now that the environment and database have been configured,
        // the database needs to be put into a deterministic testing state.
        // _connectionString MUST HAVE BEEN SET before the following can work
        
        using var dbContext = CreateDbContext();
        
        if (!useTestcontainers)
        {
            // If Testcontainers isn't being used, there may be pre-existing state to deal with.
            // Deleting it first is a good way to get a clean slate to start with
            dbContext.Database.EnsureDeleted();
        }

        // .Migrate() creates the database first if it doesn't exist, which is good if .EnsureDeleted() was run
        dbContext.Database.Migrate();
        dbContext.SeedTestData();
    }
}