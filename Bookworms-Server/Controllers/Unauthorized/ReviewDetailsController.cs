using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Book Reviews - Unauthorized")]
[Produces("application/json")]
public class ReviewDetailsController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Gets reviews for the specified book
    /// </summary>
    /// <param name="bookId">The Google Books ID of the book to target</param>
    /// <param name="start" default="0">The start index from which to start returning reviews (first is 0)</param>
    /// <param name="max" default="-1">The maximum number of reviews to return (use -1 for unconstrained)</param>
    /// <returns>The list of reviews</returns>
    /// <response code="200">Success</response>
    /// <response code="404">The book is not found</response>
    [HttpGet]
    [Route("/books/{bookId}/reviews")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ReviewResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Get(string bookId, int start, int max)
    {
        Book? book = dbContext.Books
            .Include(book => book.Reviews)
            .FirstOrDefault(b => b.BookId == bookId);

        if (book is null)
        {
            return NotFound(ErrorResponse.BookNotFound);
        }

        IQueryable<Review> q = dbContext.Reviews
            .Include(review => review.Reviewer)
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.ReviewDate)
            .Skip(start);
        
        if (max > 0)
            q = q.Take(max);

        List<ReviewResponse> output = q.AsEnumerable().Select(review => review.ToResponse()).ToList();
            
        return Ok(output);
    }
}