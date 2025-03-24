using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Recommend")]
[Route("/Recommend/[action]")]
public class RecommendationController(BookwormsDbContext context): AuthControllerBase(context)
{
    private const double PositiveReviewThreshold = 3.0;

    /// <summary>
    /// Gets a list of books by the same authors the user has left positive reviews for
    /// </summary>
    /// <returns>A list of books by the same authors the user has left positive reviews for</returns>
    /// <response code="200">Returns a list of books by the same authors the user has left positive reviews for</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child ID is invalid, or is not managed by the logged-in user</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult SameAuthors(string? childId = null)
    {
        if (!string.IsNullOrEmpty(childId))
        {
            if (CurrentUser is not Parent)
            {
                return Forbidden(ErrorResponse.UserNotParent);
            }
            if (CurrentUserChild(childId) is not {})
            {
                return NotFound(ErrorResponse.ChildNotFound);
            }
        }

        List<string> reviewedBooks = DbContext.Reviews
            .Where(review => review.Reviewer.Username == CurrentUser.Username)
            .Select(review => review.BookId)
            .ToList();

        List<string> positivelyReviewedBooks = DbContext.Reviews
            .Where(review => review.Reviewer.Username == CurrentUser.Username && review.StarRating >= PositiveReviewThreshold)
            .Select(review => review.BookId)
            .ToList();

        List<List<string>> sameAuthorsList = DbContext.Books
            .Where(book => positivelyReviewedBooks.Contains(book.BookId))
            .Select(book => book.Authors)
            .Distinct()
            .ToList();

        List<string> sameAuthors = sameAuthorsList.SelectMany(authors => authors).Distinct().ToList();

        List<string> formattedSameAuthors = sameAuthors.Select(author => $"%\"{author}\"%").ToList();

        List<string> booksSameAuthors = DbContext.Books
            .Where(book => formattedSameAuthors.Any(author => EF.Functions.Like((string)(object)book.Authors, author)) && !reviewedBooks.Contains(book.BookId))
            .Select(book => book.BookId)
            .ToList();

        if (booksSameAuthors.Count > 10)
        {
            var random = new Random();
            booksSameAuthors = booksSameAuthors.OrderBy(x => random.Next()).Take(10).ToList();
        }

        List<Book> books = DbContext.Books.Where(book => booksSameAuthors.Contains(book.BookId)).ToList();
        List<BookResponse> bookResponses = books.Select(book => book.ToResponse()).ToList();

        return Ok(bookResponses);
    }

    /// <summary>
    /// Gets a list of books with similar descriptions to other books the user has left positive reviews for
    /// </summary>
    /// <returns>A list of books with similar descriptions to other books the user has left positive reviews for</returns>
    /// <response code="200">Returns a list ofbooks with similar descriptions to other books the user has left positive reviews for</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child ID is invalid, or is not managed by the logged-in user</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult SimilarDescriptions(string? childId = null)
    {
        if (!string.IsNullOrEmpty(childId))
        {
            if (CurrentUser is not Parent)
            {
                return Forbidden(ErrorResponse.UserNotParent);
            }
            if (CurrentUserChild(childId) is not {})
            {
                return NotFound(ErrorResponse.ChildNotFound);
            }
        }

        List<string> reviewedBooks = DbContext.Reviews
            .Where(review => review.Reviewer.Username == CurrentUser.Username)
            .Select(review => review.BookId)
            .ToList();

        List<string> positivelyReviewedBooks = DbContext.Reviews
            .Where(review => review.Reviewer.Username == CurrentUser.Username && review.StarRating >= PositiveReviewThreshold)
            .Select(review => review.BookId)
            .ToList();

        List<List<string>?> similarBooksList = DbContext.Books
            .Where(book => positivelyReviewedBooks.Contains(book.BookId))
            .Select(book => book.SimilarBooks)
            .Distinct()
            .ToList();

        List<string> similarBooks = similarBooksList.SelectMany(books => books ?? []).Where(book => !reviewedBooks.Contains(book)).Distinct().ToList();

        if (similarBooks.Count > 10)
        {
            var random = new Random();
            similarBooks = similarBooks.OrderBy(x => random.Next()).Take(10).ToList();
        }

        List<Book> books = DbContext.Books.Where(book => similarBooks.Contains(book.BookId)).ToList();
        List<BookResponse> bookResponses = books.Select(book => book.ToResponse()).ToList();

        return Ok(bookResponses);
    }
}