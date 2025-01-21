using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookDetailsTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    // TODO - This test sometimes fails due to remote issues. Need to figure out game plan for remote API
    [Theory]
    [InlineData("VnAkAQAAMAAJ", "9780689204531")]
    [InlineData("1IleAgAAQBAJ", "9780061965104")]
    public async Task Test_GetBookDetails(string bookId, string isbn13)
    {
        // TODO - Workaround for GoogleBooks API going down
        HttpClient c = new HttpClient();
        HttpResponseMessage responsePre = await c.GetAsync($"https://www.googleapis.com/books/v1/volumes/{bookId}");
        if (responsePre.StatusCode != HttpStatusCode.TooManyRequests)
        {
            HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/details");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            BookDetailsDTO? content = await response.Content.ReadJsonAsync<BookDetailsDTO>();

            Assert.NotNull(content);
            Assert.False(content.Description.IsNullOrEmpty());
            Assert.Equal(isbn13, content.Isbn13);
        }
    }

    [Theory]
    [InlineData("abc123")]
    public async Task Test_GetBookDetails_InvalidBookId(string bookId)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/details");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        
        ErrorDTO? content = await response.Content.ReadJsonAsync<ErrorDTO>();
        
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.BookNotFound, content);
    }
    
    [Theory]
    [InlineData("1IleAgAAQBAJ", "0c2df2776ed3100cd82113b6c39c7ec3")]
    [InlineData("VnAkAQAAMAAJ", "83bf16254e2a31531fd2c58421bad407")]
    public async Task Test_GetImage(string bookId, string md5Hash)
    {
        HttpResponseMessage response = await Client.GetAsync($"/books/{bookId}/cover");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        byte[] content = await response.Content.ReadAsByteArrayAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
        using MD5 md5 = MD5.Create();
        md5.TransformFinalBlock(content, 0, content.Length);
        Assert.Equal(md5Hash, BitConverter.ToString(md5.Hash!).Replace("-", string.Empty).ToLower());
    }
    
    [Theory]
    [InlineData("1IleAgAAQBAJ", "0c2df2776ed3100cd82113b6c39c7ec3","VnAkAQAAMAAJ", "83bf16254e2a31531fd2c58421bad407")]
    public async Task Test_GetImageBatch(string bookId1, string md5Hash1, string bookId2, string md5Hash2)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/books/covers", new List<string>([bookId1, bookId2]));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        byte[] content = await response.Content.ReadAsByteArrayAsync();
        Assert.NotNull(content);
        Assert.NotEmpty(content);
        
        List<string> hashes = [md5Hash1, md5Hash2];
        Stream stream = new MemoryStream(content);
        using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

        for (int i = 0; i < archive.Entries.Count; i++)
        {
            ZipArchiveEntry entry = archive.Entries[i];
            await using Stream entryStream = entry.Open();
            using MemoryStream memoryStream = new MemoryStream();
            await entryStream.CopyToAsync(memoryStream);
            
            memoryStream.Seek(0, SeekOrigin.Begin);
        
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = await md5.ComputeHashAsync(memoryStream);
            string hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToLower();
            Assert.Equal(hashes[i], hashString);
        }
    }
}