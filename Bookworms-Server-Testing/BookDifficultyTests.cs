using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookDifficultyTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Theory]
    [InlineData("parent3", Constants.Parent3Child2Id, "OL2191470M", 4)]
    public async Task Test_AddDifficultyRating_UpdatesBook(string username, string childId, string bookId, int rating)
    {
        Child child = (await this.Context.Children.FindAsync(childId))!;
        int? oldChildLevel = child.ReadingLevel;
        
        Book book = Context.Books
            .Include(b => b.DifficultyRatings)
            .FirstOrDefault(b => b.BookId == bookId)!;
        int oldNumRatings = book.DifficultyRatings.Count;
        int? oldBookLevel = book.Level;
        
        await CheckResponse<UpdatedLevelResponse>(
            async () => await Client.PostPayloadAsync(Routes.RateDifficulty(bookId),
                new DifficultyRatingAddRequest(childId, rating), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal("book", content.EntityTypeName);
                Assert.Equal(bookId, content.EntityId);
                Assert.Equal(oldBookLevel, content.OldLevel);
                // The book didn't have any ratings to begin with,
                // so the new level should be entirely based on the one rating
                Assert.Equal(child.ReadingLevel + 3 * (rating - 3), content.NewLevel);
            }
        );
        
        Assert.Equal(oldNumRatings + 1, book.DifficultyRatings.Count);
        Assert.Equal(oldChildLevel, child.ReadingLevel);
    }
    
    [Theory]
    [InlineData("parent2", Constants.Parent2Child1Id, "OL3368288W", 1)]
    [InlineData("parent2", Constants.Parent2Child1Id, "OL3368288W", 2)]
    [InlineData("parent2", Constants.Parent2Child1Id, "OL3368288W", 3)]
    [InlineData("parent2", Constants.Parent2Child1Id, "OL3368288W", 4)]
    [InlineData("parent2", Constants.Parent2Child1Id, "OL3368288W", 5)]
    public async Task Test_AddDifficultyRating_UpdatesChild(string username, string childId, string bookId, int rating)
    {
        Child child = (await this.Context.Children.FindAsync(childId))!;
        int? oldChildLevel = child.ReadingLevel;
        
        Book book = Context.Books
            .Include(b => b.DifficultyRatings)
            .FirstOrDefault(b => b.BookId == bookId)!;
        int oldNumRatings = book.DifficultyRatings.Count;
        book.Level = 50;
        int? oldBookLevel = book.Level;
        book.LevelIsLocked = true;
        
        await CheckResponse<UpdatedLevelResponse>(
            async () => await Client.PostPayloadAsync(Routes.RateDifficulty(bookId),
                new DifficultyRatingAddRequest(childId, rating), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal("child", content.EntityTypeName);
                Assert.Equal(childId, content.EntityId);
                Assert.Equal(oldChildLevel, content.OldLevel);
                // updated child level is harder to assert than I want to deal with since my formula is a bit wack
            }
        );
        
        Assert.Equal(oldNumRatings, book.DifficultyRatings.Count);
        Assert.Equal(oldBookLevel, book.Level);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "OL2191470M", 4)]
    public async Task Test_AddDifficultyRating_UpdatesNeither(string username, string childId, string bookId, int rating)
    {
        Child child = (await this.Context.Children.FindAsync(childId))!;
        int? oldChildLevel = child.ReadingLevel;
        
        Book book = Context.Books
            .Include(b => b.DifficultyRatings)
            .FirstOrDefault(b => b.BookId == bookId)!;
        int oldNumRatings = book.DifficultyRatings.Count;
        int? oldBookLevel = book.Level;
        
        await CheckResponse(async () => await Client.PostPayloadAsync(Routes.RateDifficulty(bookId),
                new DifficultyRatingAddRequest(childId, rating), username),
            HttpStatusCode.NoContent
        );
        
        Assert.Equal(oldNumRatings, book.DifficultyRatings.Count);
        Assert.Equal(oldChildLevel, child.ReadingLevel);
        Assert.Equal(oldBookLevel, book.Level);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "InvalidBookId", 4)]
    public async Task Test_AddDifficultyRating_InvalidBook(string username, string childId, string bookId, int rating)
    {
        Child child = (await this.Context.Children.FindAsync(childId))!;
        int? oldChildLevel = child.ReadingLevel;
        
        await CheckForError(
            async () => await Client.PostPayloadAsync(Routes.RateDifficulty(bookId),
                new DifficultyRatingAddRequest(childId, rating), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookNotFound
        );
        
        Assert.Equal(oldChildLevel, child.ReadingLevel);
    }
    
    [Theory]
    [InlineData("parent1", "InvalidChildId", "OL3368288W", 4)]
    public async Task Test_AddDifficultyRating_InvalidChild(string username, string childId, string bookId, int rating)
    {
        Book book = Context.Books
            .Include(b => b.DifficultyRatings)
            .FirstOrDefault(b => b.BookId == bookId)!;
        int oldNumRatings = book.DifficultyRatings.Count;
        int? oldBookLevel = book.Level;
        
        await CheckForError(
            async () => await Client.PostPayloadAsync(Routes.RateDifficulty(bookId),
                new DifficultyRatingAddRequest(childId, rating), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound
        );
        
        Assert.Equal(oldNumRatings, book.DifficultyRatings.Count);
        Assert.Equal(oldBookLevel, book.Level);
    }

    [Theory]
    [InlineData("teacher1", Constants.Parent1Child1Id, "OL3368288W", 4)]
    public async Task Test_AddDifficultyRating_UserIsNotParent(string username, string childId, string bookId, int rating)
    {
        Book book = Context.Books
            .Include(b => b.DifficultyRatings)
            .FirstOrDefault(b => b.BookId == bookId)!;
        int oldNumRatings = book.DifficultyRatings.Count;
        int? oldBookLevel = book.Level;
        
        await CheckForError(
            async () => await Client.PostPayloadAsync(Routes.RateDifficulty(bookId),
                new DifficultyRatingAddRequest(childId, rating), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent
        );
        
        Assert.Equal(oldNumRatings, book.DifficultyRatings.Count);
        Assert.Equal(oldBookLevel, book.Level);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent2Child1Id, "OL3368288W", 4)]
    public async Task Test_AddDifficultyRating_ChildDoesNotBelongToParent(string username, string childId, string bookId, int rating)
    {
        Book book = Context.Books
            .Include(b => b.DifficultyRatings)
            .FirstOrDefault(b => b.BookId == bookId)!;
        int oldNumRatings = book.DifficultyRatings.Count;
        int? oldBookLevel = book.Level;
        
        await CheckForError(
            async () => await Client.PostPayloadAsync(Routes.RateDifficulty(bookId),
                new DifficultyRatingAddRequest(childId, rating), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound
        );
        
        Assert.Equal(oldNumRatings, book.DifficultyRatings.Count);
        Assert.Equal(oldBookLevel, book.Level);
    }
}