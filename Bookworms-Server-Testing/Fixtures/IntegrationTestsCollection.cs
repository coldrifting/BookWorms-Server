namespace BookwormsServerTesting.Fixtures;

/// <summary>
/// Collection definition for integration tests.
/// <br/>
/// Implements ICollectionFixture to include a CompositeFixture object as a collection-scope fixture.
/// </summary>
[CollectionDefinition("Integration Tests")]
public class IntegrationTestsCollection : ICollectionFixture<CompositeFixture>;