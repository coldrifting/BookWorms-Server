using BookwormsServer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BookwormsServerTesting.Templates;

public abstract class BaseTestReadOnlyFixture(WebApplicationFactory<Program> factory) : IAsyncLifetime, IClassFixture<AppFactory<Program>>
{
    protected HttpClient Client = null!;
    protected AsyncServiceScope Scope;
    protected BookwormsDbContext Context = null!;

    public virtual Task InitializeAsync()
    {
        Client = factory.CreateClient();
        Client.DefaultRequestHeaders.Add("Accept", "application/json");
        Scope = factory.Services.CreateAsyncScope();
        Context = Scope.ServiceProvider.GetService<BookwormsDbContext>()!;
        
        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        await Scope.DisposeAsync();
    }
}

public abstract class BaseTestWriteFixture(WebApplicationFactory<Program> factory) : BaseTestReadOnlyFixture(factory)
{
    public override async Task DisposeAsync()
    {
        Context.SeedTestData();
        
        await Scope.DisposeAsync();
    }
}