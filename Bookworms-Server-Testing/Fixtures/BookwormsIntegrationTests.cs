using System.Data;
using BookwormsServer;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace BookwormsServerTesting.Fixtures;

/// <summary>
/// Base class for Integration Tests.
/// <br/>
/// This class makes an HTTP Client and a DBContext available from the incoming collection fixture.
/// It also takes care of wrapping tests in READ COMMITTED database transactions, so the database
/// doesn't have to be dropped and recreated between each test; transactions can simply be rolled back.
/// <br/>
/// As a base class for test classes, this class is created (and therefore the setup/teardown methods are called)
/// for EVERY INDIVIDUAL TEST, not once per test class.  
/// </summary>
public class BookwormsIntegrationTests : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly BookwormsDbContext Context;
    private IDbContextTransaction _transaction = null!;

    // Called before every test. Each test gets its own DbContext and HttpClient
    protected BookwormsIntegrationTests(CompositeFixture fixture)
    {
        this.Context = fixture.Database.CreateDbContext();
        this.Client = fixture.Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_ => this.Context);
            });
        })
        .CreateClient();
        Client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // Called before every test. Each test gets its own transaction
    public async Task InitializeAsync()
    {
        this._transaction = await this.Context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
    }

    // Called after every test
    public async Task DisposeAsync()
    {
        await this._transaction.RollbackAsync();
        await this.Context.DisposeAsync();
        this.Client.Dispose();
    }
}