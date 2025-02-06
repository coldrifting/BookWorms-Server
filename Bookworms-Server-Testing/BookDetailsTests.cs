using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Helpers;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Security.Cryptography;
using BookwormsServerTesting.Fixtures;
using Microsoft.IdentityModel.Tokens;

using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookDetailsTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Theory]
    [InlineData("OL3368288W", "The Giving Tree", new[] {"Shel Silverstein"}, 2.9)]
    [InlineData("OL21025297W", "Magic of Giving", new[] {"Marc Dunston","Katie Cantrell","Wally Amos"}, 3.17)]
    [InlineData("OL2191470M", "The Magic School Bus: lost in the solar system", new[] {"Joanna Cole"}, null)]
    public async Task Test_GetBookDetailsBasic(string bookId, string title, string[] authors, double? rating)
    {
        await CheckResponse<BookDetailsDTO>(async () => await Client.GetAsync(Routes.Books.Details(bookId)),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(bookId, content.BookId);
                Assert.Equal(title, content.Title);
                Assert.Equal(authors, content.Authors);
                Assert.Equal(rating, content.Rating);
            });
    }
    
    [Theory]
    [InlineData("OL3368288W", "The Giving Tree", new[] {"Shel Silverstein"}, 2.9, "New York Times", new[] {"Fiction", "American"}, "0060256656", null, 1964, 57, 5)]
    [InlineData("OL47935W", "While the Clock Ticked", new[] {"Franklin W. Dixon"}, 2.67, "mystery of the secret", new[] {"Hardy boys (fictitious characters)"}, "1557092699", "9781557092694", 1932, 212, 3)]
    [InlineData("OL48763W", "The three robbers", new[] {"Tomi Ungerer"}, 2.75, "terrify the countryside", new[] {"Robbers and outlaws","Juvenile fiction"}, "1570982066", null, 1981, null, 4)]
    [InlineData("OL8843356W", "The Giving Book", new[] {"Ellen Sabin"}, 3.7, "",  new string[] {}, "0545084504", "9780545084505", 2004, 46, 5)]
    public async Task Test_GetBookDetailsExtended(string bookId, string title, string[] authors, double? rating,
                                                  string description, string[] subjects, string? isbn10, string? isbn13,
                                                  int publishYear, int? pageCount, int numReviews)
    {
        await CheckResponse<BookDetailsExtendedDTO>(async () => await Client.GetAsync(Routes.Books.Details(bookId, true)),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(bookId, content.BookId);
                Assert.Equal(title, content.Title);
                Assert.Equal(authors, content.Authors);
                Assert.Equal(rating, content.Rating);
                Assert.Contains(description, content.Description);
                
                Assert.True(subjects.All(subjSubString => content.Subjects.Any(bookSubj => bookSubj.Contains(subjSubString))));

                Assert.Equal(isbn10, content.Isbn10);
                Assert.Equal(isbn13, content.Isbn13);
                Assert.Equal(publishYear, content.PublishYear);
                Assert.Equal(pageCount, content.PageCount);
                Assert.Equal(numReviews, content.Reviews.Count);
            });
    }

    [Theory]
    [InlineData("abc123", true)]
    [InlineData("abc123", false)]
    public async Task Test_GetBookDetails_InvalidBookId(string bookId, bool extended)
    {
        await CheckForError(() => Client.GetAsync(Routes.Books.Details(bookId, extended)), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookNotFound);
    }
    
    [Theory]
    [InlineData("OL3368288W", "f6269ead8320cd8c5c2bfac121fe0019")]
    [InlineData("OL48763W", "cc278287bca068865216ecbcc8fa37ab")]
    public async Task Test_GetImage(string bookId, string md5Hash)
    {
        HttpResponseMessage response = await Client.GetAsync(Routes.Books.Cover(bookId));
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
        HttpResponseMessage response = await Client.PostAsJsonAsync(Routes.Books.CoverBatch, new List<string>([bookId1, bookId2]));
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