using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookDetailsTests(AppFactory<Program> factory) : BaseTestReadOnlyFixture(factory)
{
    [Theory]
    [InlineData("OL3368288W", "0060256656")]
    [InlineData("OL48763W", "1570982066")]
    public async Task Test_GetBookDetails(string bookId, string isbn10)
    {
        await CheckResponse<BookDetailsDTO>(async () => await Client.GetAsync($"/books/{bookId}/details"),
            HttpStatusCode.OK,
            content =>
            {
                Assert.False(content.Description.IsNullOrEmpty());
                Assert.Equal(isbn10, content.Isbn10);
            });
    }

    [Theory]
    [InlineData("abc123")]
    public async Task Test_GetBookDetails_InvalidBookId(string bookId)
    {
        await CheckForError(() => Client.GetAsync($"/books/{bookId}/details"), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookNotFound);
    }
    
    [Theory]
    [InlineData("OL3368288W", "f6269ead8320cd8c5c2bfac121fe0019")]
    [InlineData("OL48763W", "cc278287bca068865216ecbcc8fa37ab")]
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
    [InlineData("OL3368288W", "f6269ead8320cd8c5c2bfac121fe0019","OL48763W", "cc278287bca068865216ecbcc8fa37ab")]
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