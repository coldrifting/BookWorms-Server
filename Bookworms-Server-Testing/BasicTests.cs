using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Templates;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BasicTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    [Theory]
    [InlineData("VnAkAQAAMAAJ", "The Three Robbers")]
    [InlineData("1IleAgAAQBAJ", "The Giving Tree")]
    public async Task TestGet_BookDetails(string bookId, string title)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/details");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<BookDetailsDTO>();

        Assert.NotNull(content);
        Assert.Equal(title, content.Title);
    }
    
    [Theory]
    [InlineData("The Three R", "The Three Robbers")]
    [InlineData("Giving", "The Giving Tree")]
    public async Task TestGet_SearchResults(string searchString, string title)
    {
        HttpResponseMessage response = await Client.GetAsync($"/search/title?query={searchString}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<BookDto>>();

        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, bookDto => bookDto.Title == title);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "parent0", "I like green")]
    public async Task TestGet_AllReviews(string bookId, string username, string reviewText)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>();
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, r => r != null && r.ReviewerUsername == username && r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "teacher1", 3.5, "some review text")]
    public async Task TestPost_Review(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync($"/books/{bookId}/review", 
            new ReviewDTO(username, username, rating, reviewText));
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<Review>();
        
        Assert.NotNull(content);
        Assert.Equal(username, content.Username);
        
        HttpResponseMessage response2 = await Client.GetAsync($"/reviews/{content.ReviewId}");
        
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var content2 = await response2.Content.ReadFromJsonAsync<ReviewDTO>();

        Assert.NotNull(content2);
        Assert.Equal(content2.ReviewerUsername, username);
        Assert.Equal(content2.ReviewText, reviewText);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var content3 = await response3.Content.ReadFromJsonAsync<List<ReviewDTO>>();

        Assert.NotNull(content3);
        Assert.NotEmpty(content3);
        Assert.Contains(content3, r => r.ReviewText == reviewText && r.ReviewerUsername == username);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ",  0, -1, 7)]
    [InlineData("1IleAgAAQBAJ",  1, 3, 3)]
    [InlineData("1IleAgAAQBAJ",  5, 3, 2)]
    public async Task TestGet_ReviewsByBook(string bookId, int start, int max, int expected)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start={start}&max={max}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>();
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Equal(expected, content.Count);
    }
}