using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookSearchTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    [Theory]
    [InlineData("The Three R", "The Three Robbers")]
    [InlineData("Giving", "The Giving Tree")]
    public async Task Test_GetSearchResults(string searchString, string title)
    {
        HttpResponseMessage response = await Client.GetAsync($"/search/title?query={searchString}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadJsonAsync<List<BookDto>>();

        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, bookDto => string.Equals(bookDto.Title, title, StringComparison.OrdinalIgnoreCase));
    }
    
    // TODO - Add more tests when more search parameters are added
}