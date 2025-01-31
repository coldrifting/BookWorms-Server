using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Templates;
using Microsoft.AspNetCore.Mvc.Testing;
using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookSearchTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Theory]
    [InlineData("The Three R", "The Three Robbers")]
    [InlineData("Giving", "The Giving Tree")]
    public async Task Test_GetSearchResults(string searchString, string title)
    {
        await CheckResponse<List<BookDTO>>(async () => await Client.GetAsync(Routes.Search.Title(searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDto => string.Equals(bookDto.Title, title, StringComparison.OrdinalIgnoreCase));
            });
    }
    
    // TODO - Add more tests when more search parameters are added
}