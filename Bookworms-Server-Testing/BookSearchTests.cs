using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;

using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class BookSearchTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Theory]
    [InlineData("The Three R", "The Three Robbers", 2.75)]
    [InlineData("Giving", "The Giving Tree", 2.9)]
    public async Task Test_GetSearchResults(string searchString, string title, double rating)
    {
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.Title, title, StringComparison.OrdinalIgnoreCase) &&
                                                    bookDTO.Rating != null && 
                                                    Math.Abs(bookDTO.Rating.Value - rating) < 0.025);
            });
    }
    
    [Theory]
    [InlineData("Magic", "OL2191470M")]
    public async Task Test_GetSearchResultsNoReviews(string searchString, string bookId)
    {
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => bookDTO.BookId == bookId && bookDTO.Rating == null);
            });
    }
    
    [Theory]
    [InlineData("parent2", "The Three R", "OL48763W", 5.0, 3.2)]
    [InlineData("teacher1", "Giving", "OL3368288W", 1.5, 2.67)]
    public async Task Test_GetSearchResultsAfterLeavingNewReviewRating(string username, string searchString, string bookId, double newRating, double rating)
    {
        await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId), new ReviewEditRequest(newRating), username);
        
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId) &&
                                                    bookDTO.Rating != null &&
                                                    Math.Abs(bookDTO.Rating.Value - rating) < 0.025);
            });
    }
    
    [Theory]
    [InlineData("parent1", "The Three R", "OL48763W", 5.0, 3.125)]
    [InlineData("teacher2", "Giving", "OL3368288W", 1.5, 3.1)]
    [InlineData("teacher2", "Giving", "OL3368288W", 0, 2.8)]
    public async Task Test_GetSearchResultsAfterUpdatingReviewRating(string username, string searchString, string bookId, double newRating, double rating)
    {
        await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId), new ReviewEditRequest(newRating), username);
        
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId) && 
                                                    bookDTO.Rating != null &&
                                                    Math.Abs(bookDTO.Rating.Value - rating) < 0.025);
            });
    }
    
    [Theory]
    [InlineData("parent1", "The Three R", "OL48763W", 2.5)]
    [InlineData("teacher2", "Giving", "OL3368288W", 3.5)]
    public async Task Test_GetSearchResultsAfterDeletingReview(string username, string searchString, string bookId, double rating)
    {
        await Client.DeleteAsync(Routes.Reviews.Remove(bookId), username);
        
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId) &&
                                                    bookDTO.Rating != null &&
                                                    Math.Abs(bookDTO.Rating.Value - rating) < 0.025);
            });
    }
    
    [Theory]
    [InlineData("parent2", "Magic", "OL2191470M", 5.0)]
    [InlineData("teacher1", "Magic", "OL2191470M", 0.0)]
    [InlineData("parent0", "Magic", "OL2191470M", 0.5)]
    [InlineData("teacher2", "Magic", "OL2191470M", 2.16)]
    public async Task Test_GetSearchResultsAfterLeavingFirstNewReviewRating(string username, string searchString, string bookId, double newRating)
    {
        await Client.PutPayloadAsync(Routes.Reviews.Remove(bookId), new ReviewEditRequest(newRating), username);
        
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId) && 
                                                    bookDTO.Rating != null &&
                                                    Math.Abs(bookDTO.Rating.Value - newRating) < 0.025);
            });
    }
    
    [Theory]
    [InlineData("parent2", "Magic", "OL2191470M", 5.0, 2.5)]
    [InlineData("teacher1", "Magic", "OL2191470M", 0.0, 0.1)]
    [InlineData("parent0", "Magic", "OL2191470M", 0.5, 0.0)]
    [InlineData("teacher2", "Magic", "OL2191470M", 2.16, 4.25)]
    public async Task Test_GetSearchResultsAfterUpdatingOnlyReviewRating(string username, string searchString, string bookId, double firstRating, double secondRating)
    {
        await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId), new ReviewEditRequest(firstRating), username);
        
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId) &&
                                                    bookDTO.Rating != null &&
                                                    Math.Abs(bookDTO.Rating.Value - firstRating) < 0.025);
            });
        
        await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId), new ReviewEditRequest(secondRating), username);
        
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId) &&
                                                    bookDTO.Rating != null &&
                                                    Math.Abs(bookDTO.Rating.Value - secondRating) < 0.025);
            });
    }
    
    [Theory]
    [InlineData("parent2", "Magic", "OL2191470M", 5.0)]
    [InlineData("teacher1", "Magic", "OL2191470M", 0.0)]
    [InlineData("parent0", "Magic", "OL2191470M", 0.5)]
    [InlineData("teacher2", "Magic", "OL2191470M", 2.16)]
    public async Task Test_GetSearchResultsAfterRemovingOnlyReviewRating(string username, string searchString, string bookId, double newRating)
    {
        await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId), new ReviewEditRequest(newRating), username);
        await Client.DeleteAsync(Routes.Reviews.Remove(bookId), username);
        
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(title: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId) && bookDTO.Rating == null);
            });
    }
    
    [Theory]
    [InlineData("Shel Silverstein", "OL3368286W", 3)]
    [InlineData("Shel", "OL3368286W", 3)]
    [InlineData("Silverstein", "OL3368286W", 3)]
    [InlineData("shel", "OL3368286W", 3)]
    [InlineData("silverstein", "OL3368286W", 3)]
    [InlineData("isa rog", "OL14912086W", 1)]
    public async Task Test_SearchAuthorBasic(string searchString, string bookId, int numResults)
    {
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(author: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Equal(numResults, content.Count);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId));
            });
    }
    
    [Theory]
    [InlineData("shel", "OL3368286W", 4)]
    public async Task Test_SearchQueryBasic(string searchString, string bookId, int numResults)
    {
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(query: searchString)),
            HttpStatusCode.OK,
            content => {
                Assert.NotEmpty(content);
                Assert.Equal(numResults, content.Count);
                Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId));
            });
    }
    
    [Theory]
    [InlineData(0, "OL3368273W", 12)]
    [InlineData(1, "OL3368273W", 12)]
    [InlineData(2.8, "OL14912086W", 7)]
    [InlineData(3.7, "OL8843356W", 1)]
    [InlineData(4.5, "", 0)]
    public async Task Test_SearchReviewRatingMinBasic(double minReview, string bookId, int numResults)
    {
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(ratingMin: minReview)),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(numResults, content.Count);
                if (numResults > 0)
                {
                    Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId));
                }
            });
    }
    
    [Theory]
    [InlineData(new [] {""}, "OL8843356W", 13)]
    [InlineData(new [] {"Fiction"}, "OL2191470M", 11)]
    [InlineData(new [] {"fiction"}, "OL3368273W", 11)]
    [InlineData(new [] {"American"}, "OL14912086W", 3)]
    [InlineData(new [] {"American", "animal"}, "OL26571192W", 4)]
    public async Task Test_SearchSubjectsBasic(string[] subjects, string bookId, int numResults)
    {
        await CheckResponse<List<BookResponse>>(async () => await Client.GetAsync(Routes.Search(subjects: subjects.ToList())),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(numResults, content.Count);
                if (numResults > 0)
                {
                    Assert.Contains(content, bookDTO => string.Equals(bookDTO.BookId, bookId));
                }
            });
    }
    
    // TODO - Add Reading Level Search tests when that system goes online
}