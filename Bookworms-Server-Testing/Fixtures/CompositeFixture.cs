using BookwormsServer;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookwormsServerTesting.Fixtures;

/// <summary>
/// Wrapper class for integration test fixtures. Uses the constructor-and-dispose pattern.
/// <br/>
/// Using a wrapper class makes it so only one fixture object needs to pass through the test classes'
/// inherited constructor hierarchy, which is much more clean and extensible. 
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class CompositeFixture: IDisposable
{
    public readonly WebApplicationFactory<Program> Factory = new();
    public readonly DatabaseFixture Database = new();

    public void Dispose()
    {
        Factory.Dispose();
        Database.Dispose();
        GC.SuppressFinalize(this);
    }
}