using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Templates;
using Microsoft.AspNetCore.Mvc.Testing;
using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

public abstract class BookReviewTests
{
    [Collection("Integration Tests")]
    public class BookReviewReadOnlyTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
    {
        [Theory]
        [InlineData("OL3368288W", "I like green")]
        public async Task Test_GetAllReviews(string bookId, string reviewText)
        {
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Contains(content, r => r != null && r.ReviewText == reviewText);
                });
        }

        [Theory]
        [InlineData("OL3368288W", 3.5, "some review text")]
        public async Task Test_PutReview_NotLoggedIn(string bookId, double rating, string reviewText)
        {
            await CheckForError(
                () => Client.PutPayloadAsync(Routes.Reviews.Edit(bookId),
                    new ReviewAddOrUpdateRequestDTO(rating, reviewText)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("InvalidBookId", "teacher1", 3.5, "some review text")]
        public async Task Test_PutReview_InvalidBookId(string bookId, string username, double rating, string reviewText)
        {
            await CheckForError(
                () => Client.PutPayloadAsync(Routes.Reviews.Edit(bookId),
                    new ReviewAddOrUpdateRequestDTO(rating, reviewText), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookNotFound);
        }

        [Theory]
        [InlineData("OL3368288W")]
        public async Task Test_DeleteReview_NotLoggedIn(string bookId)
        {
            int initialSize = 
            await CheckResponse<List<ReviewDTO>, int>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => content.Count);

            await CheckForError(
                () => Client.DeleteAsync(Routes.Reviews.Edit(bookId)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
            
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.NotEmpty(content);
                    Assert.Equal(content.Count, initialSize);
                });
        }

        [Theory]
        [InlineData("InvalidBookId", "parent0")]
        [InlineData("InvalidBookId", "teacher0")]
        public async Task Test_DeleteReview_InvalidBookId(string bookId, string username)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Reviews.Edit(bookId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookNotFound);
        }

        [Theory]
        [InlineData("OL3368288W", 0, -1, 5)]
        [InlineData("OL3368288W", 1, 3, 3)]
        [InlineData("OL3368288W", 2, 4, 3)]
        public async Task Test_GetReviews_ByBook(string bookId, int start, int max, int expected)
        {
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.AllParam(bookId, start, max)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Equal(expected, content.Count);
                });
        }

        [Theory]
        [InlineData("OL3368288W", 5)]
        public async Task Test_GetReviews_ByBook_NoParams(string bookId, int expected)
        {
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Equal(expected, content.Count);
                });
        }

        [Theory]
        [InlineData("InvalidBookId", 0, -1)]
        [InlineData("InvalidBookId", 1, 3)]
        [InlineData("InvalidBookId", 5, 3)]
        public async Task Test_GetReviews_ByBook_InvalidBookId(string bookId, int start, int max)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Reviews.AllParam(bookId, start, max)),
                HttpStatusCode.NotFound,
                ErrorDTO.BookNotFound);
        }
    }

    [Collection("Integration Tests")]
    public class BookReviewWriteTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
    {
        [Theory]
        [InlineData("OL286593W", "teacher1", 4.5, "some review text")]
        public async Task Test_PutReview_ReviewAlreadyExists(string bookId, string username, double rating, string reviewText)
        {
            await CheckResponse<ReviewDTO>(
                async () => await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId),
                new ReviewAddOrUpdateRequestDTO(rating, reviewText), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(content.ReviewText, reviewText);
                    Assert.Equal(content.StarRating, rating);
                });
        }

        [Theory]
        [InlineData("OL3368288W", "parent0", 1.0, "Didn't like it")]
        [InlineData("OL3368288W", "teacher0", 1.5, "Horrible")]
        public async Task Test_PutReview_UpdateRatingAndText(string bookId, string username, double rating,
            string reviewText)
        {
            await CheckResponse<ReviewDTO>(
                async () => await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId),
                new ReviewAddOrUpdateRequestDTO(rating, reviewText), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(content.ReviewText, reviewText);
                    Assert.Equal(content.StarRating, rating);
                });
            
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Contains(content, r => r.ReviewText == reviewText);
                });
        }

        [Theory]
        [InlineData("OL3368288W", "parent0", "Audrey", "Hepburn", 1.0, "I like green")]
        [InlineData("OL3368288W", "teacher0", "Sally", "Field", 1.5, "I like trees")]
        public async Task Test_PutReview_UpdateRating(string bookId, string username, string reviewerFirstName,
            string reviewerLastName, double rating, string reviewText)
        {
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.AllParam(bookId, 0, -1)),
                HttpStatusCode.OK,
                content => {
                    ReviewDTO? originalReview = content.SingleOrDefault(r =>
                        r.ReviewerFirstName == reviewerFirstName && r.ReviewerLastName == reviewerLastName);
                    Assert.NotNull(originalReview);
                });

            await CheckResponse<ReviewDTO>(
                async () => await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId),
                    new ReviewAddOrUpdateRequestDTO(rating, reviewText), username),
                HttpStatusCode.OK,
                _ => { });

            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Contains(content, r => r.ReviewerFirstName == reviewerFirstName && Math.Abs(r.StarRating - rating) < 0.001);
                });
        }

        [Theory]
        [InlineData("OL3368288W", "parent0", "Audrey", "Hepburn", 4.5, "New text")]
        [InlineData("OL3368288W", "teacher0", "Sally", "Field", 5.0, "New text")]
        public async Task Test_PutReview_UpdateText(string bookId, string username, string reviewerFirstName,
            string reviewerLastName, double rating, string reviewText)
        {
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.AllParam(bookId, 0, -1)),
                HttpStatusCode.OK,
                content => {
                    ReviewDTO? originalReview = content.SingleOrDefault(r =>
                        r.ReviewerFirstName == reviewerFirstName && r.ReviewerLastName == reviewerLastName);
                    Assert.NotNull(originalReview);
                });

            await CheckResponse<ReviewDTO>(
                async () => await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId),
                    new ReviewAddOrUpdateRequestDTO(rating, reviewText), username),
                HttpStatusCode.OK,
                _ => { });

            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Contains(content, r => r.ReviewerFirstName == reviewerFirstName && r.ReviewText == reviewText);
                });
        }

        [Theory]
        [InlineData("OL48763W", "teacher1", "Emma", 3.5, "some review text")]
        public async Task Test_PutReview_NewReview(string bookId, string username, string firstName, double rating, string reviewText)
        {
            await CheckResponse<ReviewDTO>(
                async () => await Client.PutPayloadAsync(Routes.Reviews.Edit(bookId),
                new ReviewAddOrUpdateRequestDTO(rating, reviewText), username),
                HttpStatusCode.Created,
                content => {
                    Assert.Equal(rating, content.StarRating);
                    Assert.Equal(reviewText, content.ReviewText);
                });
            
            await CheckResponse<List<ReviewDTO>>(
                async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Contains(content, c => c.ReviewerFirstName == firstName && c.ReviewText == reviewText && Math.Abs(c.StarRating - rating) < 0.01);
                });
        }

        [Theory]
        [InlineData("OL3368288W", "parent0", "Audrey")]
        [InlineData("OL3368288W", "teacher0", "Sally")]
        public async Task Test_DeleteReview(string bookId, string username, string reviewerFirstName)
        {
             int initialSize = await CheckResponse<List<ReviewDTO>, int>(
                 async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    return content.Count;
                });
             
             await CheckResponse(
                 async () => await Client.DeleteAsync(Routes.Reviews.Edit(bookId), username), HttpStatusCode.NoContent);

             await CheckResponse<List<ReviewDTO>>(
                 async () => await Client.GetAsync(Routes.Reviews.All(bookId)),
                HttpStatusCode.OK,
                content => {
                    Assert.NotEmpty(content);
                    Assert.Equal(initialSize - 1, content.Count);
                    Assert.DoesNotContain(content, r => r.ReviewerFirstName == reviewerFirstName);
                });
        }
    }
}