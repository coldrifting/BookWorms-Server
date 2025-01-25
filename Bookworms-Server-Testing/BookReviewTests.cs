using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookReviewTests(BaseStartup<Program> factory) : BaseTest(factory)
{
   [Theory]
    [InlineData("OL3368288W", "I like green")]
    public async Task Test_GetAllReviews(string bookId, string reviewText)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, r => r != null && r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("OL3368288W", 3.5, "some review text")]
    public async Task Test_PutReview_NotLoggedIn(string bookId, double rating, string reviewText)
    {
        await CheckForError(() => Client.PutAsJsonAsync($"/books/{bookId}/review", new ReviewAddOrUpdateRequestDTO(rating, reviewText)), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("OL3368288W", "teacher1", 3.5, "some review text")]
    public async Task Test_PutReview_Basic(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsyncAsUser($"/books/{bookId}/review", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText), username);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        ReviewDTO? content = await response.Content.ReadJsonAsync<ReviewDTO>();
        Assert.NotNull(content);
        
        HttpResponseMessage response2 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var content2 = await response2.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content2);
        Assert.NotEmpty(content2);
        Assert.Contains(content2, r => r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("InvalidBookId", "teacher1", 3.5, "some review text")]
    public async Task Test_PutReview_InvalidBookId(string bookId, string username, double rating, string reviewText)
    {
        await CheckForError(() => Client.PutAsJsonAsyncAsUser($"/books/{bookId}/review", 
                new ReviewAddOrUpdateRequestDTO(rating, reviewText), username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookNotFound);
    }
    
    [Theory]
    [InlineData("OL286593W", "teacher1", 4.5, "some review text")]
    public async Task Test_PutReview_ReviewAlreadyExists(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsyncAsUser($"/books/{bookId}/review", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        ReviewDTO? content = await response.Content.ReadJsonAsync<ReviewDTO>();
        Assert.NotNull(content);
        Assert.Equal(content.ReviewText, reviewText);
        Assert.Equal(content.StarRating, rating);
    }

    [Theory]
    [InlineData("OL3368288W", "parent0", 1.0, "Didn't like it")]
    [InlineData("OL3368288W", "teacher0", 1.5, "Horrible")]
    public async Task Test_PutReview_UpdateRatingAndText(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsyncAsUser($"/books/{bookId}/review", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        HttpResponseMessage response2 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var content2 = await response2.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content2);
        Assert.NotEmpty(content2);
        Assert.Contains(content2, r => r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("OL3368288W", "parent0", "Audrey", "Hepburn", 1.0, "I like green")]
    [InlineData("OL3368288W", "teacher0", "Sally", "Field", 1.5, "I like trees")]
    public async Task Test_PutReview_UpdateRating(string bookId, string username, string reviewerFirstName, string reviewerLastName, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start=0&max=-1");
        var content = await response.Content.ReadJsonAsync<List<ReviewDTO>>();
        ReviewDTO? originalReview = content?.SingleOrDefault(r => r.ReviewerFirstName == reviewerFirstName && r.ReviewerLastName == reviewerLastName);
        Assert.NotNull(originalReview);

        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/books/{bookId}/review",
            new ReviewAddOrUpdateRequestDTO(rating, reviewText), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        var content3 = await response3.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content3);
        Assert.NotEmpty(content3);
        Assert.Contains(content3, r => Math.Abs(r.StarRating - rating) < 0.001);
    }
    
    [Theory]
    [InlineData("OL3368288W", "parent0", "Audrey", "Hepburn", 4.5, "New text")]
    [InlineData("OL3368288W", "teacher0", "Sally", "Field", 5.0, "New text")]
    public async Task Test_PutReview_UpdateText(string bookId, string username, string reviewerFirstName, string reviewerLastName, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start=0&max=-1");
        var content = await response.Content.ReadJsonAsync<List<ReviewDTO>>();
        
        ReviewDTO? originalReview = content?.SingleOrDefault(r => r.ReviewerFirstName == reviewerFirstName && r.ReviewerLastName == reviewerLastName);
        Assert.NotNull(originalReview);

        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/books/{bookId}/review",
            new ReviewAddOrUpdateRequestDTO(rating, reviewText), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var content3 = await response3.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content3);
        Assert.NotEmpty(content3);
        Assert.Contains(content3, r => r.ReviewText == reviewText);
    }
    
    [Theory]
    [InlineData("OL3368288W", "parent0", "Audrey", "Hepburn")]
    [InlineData("OL3368288W", "teacher0", "Sally", "Field")]
    public async Task Test_DeleteReview(string bookId, string username, string reviewerFirstName, string reviewerLastName)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews");
        var content = await response.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content);
        int initialSize = content.Count;
        
        HttpResponseMessage response2 = await Client.DeleteAsyncAsUser($"/books/{bookId}/review", username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        var content2 = await response3.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content2);
        Assert.NotEmpty(content2);
        Assert.DoesNotContain(content2, r => r.ReviewerFirstName == reviewerFirstName && r.ReviewerLastName == reviewerLastName);
        Assert.Equal(content2.Count, initialSize - 1);
    }

    [Theory]
    [InlineData("OL3368288W")]
    public async Task Test_DeleteReview_NotLoggedIn(string bookId)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews");
        var content = await response.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content);
        int initialSize = content.Count;
        
        await CheckForError(() => Client.DeleteAsync($"/books/{bookId}/review"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        var content3 = await response3.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content3);
        Assert.NotEmpty(content3);
        Assert.Equal(content3.Count, initialSize);
    }
    
    [Theory]
    [InlineData("InvalidBookId", "parent0")]
    [InlineData("InvalidBookId", "teacher0")]
    public async Task Test_DeleteReview_InvalidBookId(string bookId, string username)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/books/{bookId}/review", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookNotFound);
    }
    
    [Theory]
    [InlineData("OL3368288W",  0, -1, 5)]
    [InlineData("OL3368288W",  1, 3, 3)]
    [InlineData("OL3368288W",  2, 4, 3)]
    public async Task Test_GetReviews_ByBook(string bookId, int start, int max, int expected)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start={start}&max={max}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Equal(expected, content.Count);
    }
    
    [Theory]
    [InlineData("OL3368288W",   5)]
    public async Task Test_GetReviews_ByBook_NoParams(string bookId, int expected)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadJsonAsync<List<ReviewDTO>>();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Equal(expected, content.Count);
    }
    
    [Theory]
    [InlineData("InvalidBookId",  0, -1)]
    [InlineData("InvalidBookId",  1, 3)]
    [InlineData("InvalidBookId",  5, 3)]
    public async Task Test_GetReviews_ByBook_InvalidBookId(string bookId, int start, int max)
    {
        await CheckForError(() => Client.GetAsync($"/books/{bookId}/reviews?start={start}&max={max}"), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookNotFound);
    }
}