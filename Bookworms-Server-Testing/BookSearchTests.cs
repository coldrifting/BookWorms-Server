using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;

using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookSearchTests(AppFactory<Program> factory) : BaseTestReadOnlyFixture(factory)
{
    [Theory]
    [InlineData("The Three R", "The Three Robbers")]
    [InlineData("Giving", "The Giving Tree")]
    public async Task Test_GetSearchResults(string searchString, string title)
    {
        await CheckResponse<List<BookDto>>(async () => await Client.GetAsync($"/search/title?query={searchString}"),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDto => string.Equals(bookDto.Title, title, StringComparison.OrdinalIgnoreCase));
            });
    }
    
    // TODO - Add more tests when more search parameters are added
}