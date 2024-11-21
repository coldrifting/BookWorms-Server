using BookwormsServer;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MySqlConnector;

namespace BookwormsServerTesting;

// Replaces server connection string 
public class BaseStartup<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
	    // Read common app settings configuration
	    var config = new ConfigurationBuilder()
		    .SetBasePath(AppContext.BaseDirectory)
		    .AddJsonFile("appsettings.json", false, true)
		    .Build();

	    bool useDocker = config.GetValue("Database:TestContainer:Use", false);
	    if (!useDocker)
	    {
		    return;
	    }

	    string? connectionString = config.GetConnectionString("DefaultConnection");
	    string? databaseVersion = config.GetValue<string>("Database:Version");
	    
		MySqlConnectionStringBuilder connString = new MySqlConnectionStringBuilder(connectionString);
		
	    // Setup Docker container 
        var container = new ContainerBuilder()
			    .WithImage($"mysql:{databaseVersion}")
			    .WithName($"mysql-container-{Guid.NewGuid()}")
			    .WithEnvironment("MYSQL_DATABASE", connString.Database)
			    .WithEnvironment("MYSQL_ROOT_PASSWORD", connString.Password)
			    .WithPortBinding(3306, true)
			    .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
			    .Build();
		    
        Task.Run(async () => await container.StartAsync()).Wait();

        connectionString = connectionString.Replace("3306", container.GetMappedPublicPort(3306).ToString());
        
	    // Override DB Context to use docker mysql instance
		builder.UseEnvironment("Development");
	    builder.ConfigureServices(services =>
	    {
		    services.RemoveAll(typeof(DbContextOptions<AllBookwormsDbContext>));
		    
		    services.AddDbContext<AllBookwormsDbContext>(o =>
			    o.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
		    
			services.BuildServiceProvider();
	    });
    }
}
