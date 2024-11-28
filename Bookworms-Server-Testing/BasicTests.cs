using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Templates;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BasicTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    private readonly JsonSerializerOptions _jso = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    [Theory]
    [InlineData("VnAkAQAAMAAJ", "9780689204531")]
    [InlineData("1IleAgAAQBAJ", "9780061965104")]
    public async Task TestGet_BookDetails(string bookId, string isbn13)
    {
        HttpResponseMessage response = await this.Client.GetAsync($"/books/{bookId}/details");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<BookDetailsDTO>(this._jso);

        Assert.NotNull(content);
        Assert.NotNull(content.Description);
        Assert.NotEmpty(content.Description);
        Assert.Equal(isbn13, content.Isbn13);
    }

    [Theory]
    [InlineData("abc123")]
    public async Task TestGet_BookDetailsBadBookId(string bookId)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/details");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.BookNotFound, content);
    }
    
    [Theory]
    [InlineData("The Three R", "The Three Robbers")]
    [InlineData("Giving", "The Giving Tree")]
    public async Task TestGet_SearchResults(string searchString, string title)
    {
        HttpResponseMessage response = await this.Client.GetAsync($"/search/title?query={searchString}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<BookDto>>(this._jso);

        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, bookDto => bookDto.Title == title);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "I like green")]
    public async Task TestGet_AllReviews(string bookId, string reviewText)
    {
        HttpResponseMessage response = await this.Client.GetAsync($"/books/{bookId}/reviews");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(this._jso);
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, r => r != null && r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "teacher1", 3.5, "some review text")]
    public async Task TestPost_Review(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await this.Client.PostAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewCreateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ReviewDTO>(this._jso);
        Assert.NotNull(content);
        
        HttpResponseMessage response2 = await this.Client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var content2 = await response2.Content.ReadFromJsonAsync<ReviewDTO>(this._jso);
        Assert.NotNull(content2);
        Assert.Equal(content2.ReviewText, reviewText);
        Assert.Equal(content, content2);
        
        HttpResponseMessage response3 = await this.Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        var content3 = await response3.Content.ReadFromJsonAsync<List<ReviewDTO>>(this._jso);
        Assert.NotNull(content3);
        Assert.NotEmpty(content3);
        Assert.Contains(content3, r => r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("badBookId", "teacher1", 3.5, "some review text")]
    public async Task TestPost_ReviewBadBookId(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await this.Client.PostAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewCreateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.BookNotFound, content);
    }
    
    [Theory]
    [InlineData("_kaGDwAAQBAJ", "teacher1", 4.5, "some review text", 3.5, "Made us smile with its charming tale.")]
    public async Task TestPost_ReviewReviewAlreadyExists(string bookId, string username, double rating, string reviewText, double oldRating, string oldReviewText)
    {
        HttpResponseMessage response = await this.Client.PostAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewCreateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.ReviewAlreadyExists, content);
        
        Review? review = Context.Reviews
            .Include(r => r.Reviewer)
            .FirstOrDefault(r => r.BookId == bookId && 
                                        r.Reviewer!.Username == username);

        // Make sure review has not been updated
        Assert.NotNull(review);
        Assert.Equal(oldRating, review.StarRating);
        Assert.Equal(oldReviewText, review.ReviewText);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "parent0", 1.0, "Didn't like it")]
    [InlineData("1IleAgAAQBAJ", "teacher0", 1.5, "Horrible")]
    public async Task TestPut_Review(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await this.Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        HttpResponseMessage response2 = await this.Client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var content2 = await response2.Content.ReadFromJsonAsync<ReviewDTO>(this._jso);

        Assert.NotNull(content2);
        Assert.Equal(rating, content2.StarRating);
        Assert.Equal(reviewText, content2.ReviewText);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "parent0", "Audrey Hepburn")]
    [InlineData("1IleAgAAQBAJ", "teacher0", "Sally Field")]
    public async Task TestDelete_Review(string bookId, string username, string reviewerName)
    {
        HttpResponseMessage response = await this.Client.GetAsync($"/books/{bookId}/reviews");
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(this._jso);
        Assert.NotNull(content);
        var initialSize = content.Count;
        
        HttpResponseMessage response2 = await this.Client.DeleteAsync($"/books/{bookId}/review?username={username}");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await this.Client.GetAsync($"/books/{bookId}/reviews");
        var content2 = await response3.Content.ReadFromJsonAsync<List<ReviewDTO>>(this._jso);
        Assert.NotNull(content2);
        Assert.NotEmpty(content2);
        Assert.DoesNotContain(content2, r => r.ReviewerName == reviewerName);
        Assert.Equal(content2.Count, initialSize - 1);
    }
    
    [Theory]
    [InlineData("1IleAgAAQBAJ",  0, -1, 5)]
    [InlineData("1IleAgAAQBAJ",  1, 3, 3)]
    [InlineData("1IleAgAAQBAJ",  2, 4, 3)]
    public async Task TestGet_ReviewsByBook(string bookId, int start, int max, int expected)
    {
        HttpResponseMessage response = await this.Client.GetAsync($"/books/{bookId}/reviews?start={start}&max={max}");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(this._jso);
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Equal(expected, content.Count);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "0c2df2776ed3100cd82113b6c39c7ec3")]
    [InlineData("VnAkAQAAMAAJ", "83bf16254e2a31531fd2c58421bad407")]
    public async Task TestGet_Image(string bookId, string md5Hash)
    {
        HttpResponseMessage response = await this.Client.GetAsync($"/books/{bookId}/cover");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsByteArrayAsync();
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
        using var md5 = MD5.Create();
        md5.TransformFinalBlock(content, 0, content.Length);
        Assert.Equal(md5Hash, BitConverter.ToString(md5.Hash!).Replace("-", string.Empty).ToLower());
    }
}