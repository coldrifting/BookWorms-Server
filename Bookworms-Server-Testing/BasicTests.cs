using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using Microsoft.IdentityModel.Tokens;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BasicTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    private readonly JsonSerializerOptions _jso = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    // =================================================================================================================
    // Book Details
    
    [Theory]
    [InlineData("VnAkAQAAMAAJ", "9780689204531")]
    [InlineData("1IleAgAAQBAJ", "9780061965104")]
    public async Task TestGet_BookDetails(string bookId, string isbn13)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/details");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<BookDetailsDTO>(_jso);

        Assert.NotNull(content);
        Assert.False(content.Description.IsNullOrEmpty());
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
    
    // =================================================================================================================
    // Search Results
    
    [Theory]
    [InlineData("The Three R", "The Three Robbers")]
    [InlineData("Giving", "The Giving Tree")]
    public async Task TestGet_SearchResults(string searchString, string title)
    {
        HttpResponseMessage response = await Client.GetAsync($"/search/title?query={searchString}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<BookDto>>(_jso);

        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, bookDto => bookDto.Title == title);
    }

    // =================================================================================================================
    // Reviews
    
    [Theory]
    [InlineData("1IleAgAAQBAJ", "I like green")]
    public async Task TestGet_AllReviews(string bookId, string reviewText)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Contains(content, r => r != null && r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "teacher1", 3.5, "some review text")]
    public async Task TestPut_Review(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ReviewDTO>(_jso);
        Assert.NotNull(content);
        
        HttpResponseMessage response2 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var content2 = await response2.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        Assert.NotNull(content2);
        Assert.NotEmpty(content2);
        Assert.Contains(content2, r => r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("badBookId", "teacher1", 3.5, "some review text")]
    public async Task TestPut_ReviewBadBookId(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.BookNotFound, content);
    }
    
    [Theory]
    [InlineData("_kaGDwAAQBAJ", "teacher1", 4.5, "some review text")]
    public async Task TestPut_Review_ReviewAlreadyExists(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<ReviewDTO>(_jso);
        
        Assert.NotNull(content);
        Assert.Equal(content.ReviewText, reviewText);
        Assert.Equal(content.StarRating, rating);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "parent0", 1.0, "Didn't like it")]
    [InlineData("1IleAgAAQBAJ", "teacher0", 1.5, "Horrible")]
    public async Task TestPut_Review_UpdateBoth(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        HttpResponseMessage response2 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var content2 = await response2.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        Assert.NotNull(content2);
        Assert.NotEmpty(content2);
        Assert.Contains(content2, r => r.ReviewText == reviewText);
    }

    [Theory]
    [InlineData("1IleAgAAQBAJ", "parent0", "Audrey Hepburn", 1.0, "I like green")]
    [InlineData("1IleAgAAQBAJ", "teacher0", "Sally Field", 1.5, "I like trees")]
    public async Task TestPut_Review_UpdateRating(string bookId, string username, string reviewerName, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start=0&max=-1");
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        var originalReview = content?.SingleOrDefault(r => r.ReviewerName == reviewerName);
        Assert.NotNull(originalReview);

        HttpResponseMessage response2 = await Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}",
            new ReviewAddOrUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        var content3 = await response3.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        Assert.NotNull(content3);
        Assert.NotEmpty(content3);
        Assert.Contains(content3, r => Math.Abs(r.StarRating - rating) < 0.001);
    }
    
    [Theory]
    [InlineData("1IleAgAAQBAJ", "parent0", "Audrey Hepburn", 4.5, "New text")]
    [InlineData("1IleAgAAQBAJ", "teacher0", "Sally Field", 5.0, "New text")]
    public async Task TestPut_Review_UpdateReviewText(string bookId, string username, string reviewerName, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start=0&max=-1");
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        var originalReview = content?.SingleOrDefault(r => r.ReviewerName == reviewerName);
        Assert.NotNull(originalReview);

        HttpResponseMessage response2 = await Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}",
            new ReviewAddOrUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        var content3 = await response3.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        Assert.NotNull(content3);
        Assert.NotEmpty(content3);
        Assert.Contains(content3, r => r.ReviewText == reviewText);
    }
    
    [Theory]
    [InlineData("InvalidBookId", "parent0", 1.0, "Didn't like it")]
    public async Task TestPut_Review_BadBookId(string bookId, string username, double rating, string reviewText)
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync($"/books/{bookId}/review?username={username}", 
            new ReviewAddOrUpdateRequestDTO(rating, reviewText));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>(_jso);
        Assert.NotNull(content);
        Assert.Contains("Book Not Found", content.Error);
    }
    
    [Theory]
    [InlineData("1IleAgAAQBAJ", "parent0", "Audrey Hepburn")]
    [InlineData("1IleAgAAQBAJ", "teacher0", "Sally Field")]
    public async Task TestDelete_Review(string bookId, string username, string reviewerName)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews");
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        Assert.NotNull(content);
        var initialSize = content.Count;
        
        HttpResponseMessage response2 = await Client.DeleteAsync($"/books/{bookId}/review?username={username}");
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsync($"/books/{bookId}/reviews");
        var content2 = await response3.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        Assert.NotNull(content2);
        Assert.NotEmpty(content2);
        Assert.DoesNotContain(content2, r => r.ReviewerName == reviewerName);
        Assert.Equal(content2.Count, initialSize - 1);
    }
    
    [Theory]
    [InlineData("InvalidBookId", "parent0")]
    [InlineData("InvalidBookId", "teacher0")]
    public async Task TestDelete_Review_BadBookId(string bookId, string username)
    {
        HttpResponseMessage response2 = await Client.DeleteAsync($"/books/{bookId}/review?username={username}");
        Assert.Equal(HttpStatusCode.NotFound, response2.StatusCode);
        
        var content = await response2.Content.ReadFromJsonAsync<ErrorDTO>(_jso);
        Assert.NotNull(content);
        Assert.Contains("Book Not Found", content.Error);
    }
    
    [Theory]
    [InlineData("1IleAgAAQBAJ",  0, -1, 5)]
    [InlineData("1IleAgAAQBAJ",  1, 3, 3)]
    [InlineData("1IleAgAAQBAJ",  2, 4, 3)]
    public async Task TestGet_ReviewsByBook(string bookId, int start, int max, int expected)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start={start}&max={max}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<List<ReviewDTO>>(_jso);
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        Assert.Equal(expected, content.Count);
    }
    
    [Theory]
    [InlineData("InvalidBookId",  0, -1)]
    [InlineData("InvalidBookId",  1, 3)]
    [InlineData("InvalidBookId",  5, 3)]
    public async Task TestGet_ReviewsByBook_BadBookId(string bookId, int start, int max)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/reviews?start={start}&max={max}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>(_jso);
        Assert.NotNull(content);
        Assert.Contains("Book Not Found", content.Error);
    }

    // =================================================================================================================
    // Images
    
    [Theory]
    [InlineData("1IleAgAAQBAJ", "0c2df2776ed3100cd82113b6c39c7ec3")]
    [InlineData("VnAkAQAAMAAJ", "83bf16254e2a31531fd2c58421bad407")]
    public async Task TestGet_Image(string bookId, string md5Hash)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/cover");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsByteArrayAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
        using var md5 = MD5.Create();
        md5.TransformFinalBlock(content, 0, content.Length);
        Assert.Equal(md5Hash, BitConverter.ToString(md5.Hash!).Replace("-", string.Empty).ToLower());
    }
    
    [Theory]
    [InlineData("1IleAgAAQBAJ", "0c2df2776ed3100cd82113b6c39c7ec3","VnAkAQAAMAAJ", "83bf16254e2a31531fd2c58421bad407")]
    public async Task TestGet_ImageBatch(string bookId1, string md5Hash1, string bookId2, string md5Hash2)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/books/covers", new List<string>([bookId1, bookId2]));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsByteArrayAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
        List<string> hashes = [md5Hash1, md5Hash2];
        Stream stream = new MemoryStream(content);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        for (int i = 0; i < archive.Entries.Count; i++)
        {
            var entry = archive.Entries[i];
            await using var entryStream = entry.Open();
            using var memoryStream = new MemoryStream();
            await entryStream.CopyToAsync(memoryStream);
            
            memoryStream.Seek(0, SeekOrigin.Begin);
        
            using var md5 = MD5.Create();
            var hashBytes = await md5.ComputeHashAsync(memoryStream);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            Assert.Equal(hashes[i], hashString);
        }
        
    }
}