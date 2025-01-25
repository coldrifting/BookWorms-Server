using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookwormsServerTesting.Templates;

public abstract class BaseTest(WebApplicationFactory<Program> factory) 
    : IClassFixture<BaseStartup<Program>>, IAsyncLifetime
{
    private AsyncServiceScope _scope;
    private IServiceProvider _services = null!;
    protected BookwormsDbContext Context = null!;
    protected HttpClient Client = null!;
    
    public async Task InitializeAsync()
    {
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Add("Accept", "application/json");
        _scope = factory.Services.CreateAsyncScope();
        _services = _scope.ServiceProvider;
        Context = _services.GetRequiredService<BookwormsDbContext>();

        await ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }

    // The default seed data for tests that inherit from BaseTest
    // Override this class/method as needed for different types of tests requiring different DB states
    protected virtual Task ResetDatabaseAsync()
    {
        Context.SeedTestData();
        return Task.CompletedTask;
    }
}

// An example of an overriden base test class, where custom seed data can be placed
public abstract class BasicTest(WebApplicationFactory<Program> factory) : BaseTest(factory)
{
    protected override async Task ResetDatabaseAsync()
    {
        bool hasAdminUser = await Context.Users.AnyAsync(x => x.Username == "basicUser");
        if (!hasAdminUser)
        {
            User user = UserService.CreateUser("basicUser", "basicUser", "basicUserFirst", "basicUserLast", UserIcon.Icon1, true);
            Context.Users.Add(user);
        }

        await Context.SaveChangesAsync();
    }
}