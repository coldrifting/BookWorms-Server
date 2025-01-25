using BookwormsServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookwormsServerTesting.Templates;

public abstract class BaseTestReadOnlyFixture(WebApplicationFactory<Program> factory) : IAsyncLifetime, IClassFixture<AppFactory<Program>>
{
    private AsyncServiceScope _scope;
    protected HttpClient Client = null!;

    public virtual Task InitializeAsync()
    {
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _scope = factory.Services.CreateAsyncScope();
        
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await _scope.DisposeAsync();
    }
}

public abstract class BaseTestWriteFixture(WebApplicationFactory<Program> factory) : IAsyncLifetime, IClassFixture<AppFactory<Program>>
{
    private AsyncServiceScope _scope;
    private BookwormsDbContext? _context;
    protected HttpClient Client = null!;

    public virtual Task InitializeAsync()
    {
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Add("Accept", "application/json");
        
        _scope = factory.Services.CreateAsyncScope();
        _context = _scope.ServiceProvider.GetService<BookwormsDbContext>();
        
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        _context?.SeedTestData();
        
        await _scope.DisposeAsync();
    }
}