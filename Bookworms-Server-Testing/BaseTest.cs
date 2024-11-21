using BookwormsServer;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookwormsServerTesting;

public abstract class BaseTest(WebApplicationFactory<Program> factory) 
    : IClassFixture<BaseStartup<Program>>, IAsyncLifetime
{
    private AsyncServiceScope _scope;
    private IServiceProvider _services = null!;
    protected AllBookwormsDbContext Context = null!;
    protected HttpClient Client = null!;
    
    public async Task InitializeAsync()
    {
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Add("Accept", "application/json");
        _scope = factory.Services.CreateAsyncScope();
        _services = _scope.ServiceProvider;
        Context = _services.GetRequiredService<AllBookwormsDbContext>();

        await ClearDatabaseAsync();
        await SeedDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }

    private async Task ClearDatabaseAsync()
    {
        await Context.Users.ExecuteDeleteAsync();
        
        // TODO - Drop all tables
    }

    // The default seed data for tests that inherit from BaseTest
    // Override this class/method as needed for different types of tests requiring different DB states
    protected virtual async Task SeedDataAsync()
    {
        bool hasAdminUser = await Context.Users.AnyAsync(x => x.Username == "admin");
        if (!hasAdminUser)
        {
            User user = UserService.CreateUser("admin", "admin", "ad min", "admin@gmail.com");
            Context.Users.Add(user);
            
            User user2 = UserService.CreateUser("basicUser", "basicUser", "basicUser", "basicUser@gmail.com");
            Context.Users.Add(user2);
        }

        await Context.SaveChangesAsync();
    }
}

// An example of an overriden base test class, where custom seed data can be placed
public abstract class BasicTest(WebApplicationFactory<Program> factory) : BaseTest(factory)
{
    protected override async Task SeedDataAsync()
    {
        bool hasAdminUser = await Context.Users.AnyAsync(x => x.Username == "basicUser");
        if (!hasAdminUser)
        {
            User user = UserService.CreateUser("basicUser", "basicUser", "basicUser", "basicUser@gmail.com");
            Context.Users.Add(user);
        }

        await Context.SaveChangesAsync();
    }
}