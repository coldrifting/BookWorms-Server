using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Book Reviews")]
public class ReviewsController(BookwormsDbContext dbContext) : ControllerBase
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
    
    /// <summary>
    /// Adds/updates the review for the specified book and user
    /// </summary>
    /// <param name="bookId">The Google Books ID of the book to target</param>
    /// <param name="payload">The data with which to populate the new/updated review</param>
    /// <returns>The newly created or updated review</returns>
    /// <response code="200">Success. Review was Updated</response>
    /// <response code="201">Success. Review was Created</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="404">The book is not found</response>
    [HttpPut]
    [Authorize]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponse))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult AddOrUpdate(string bookId, [FromBody] ReviewEditRequest payload)
    {
        Book? book = dbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .FirstOrDefault(b => b.BookId == bookId);
        if (book is null)
        {
            return NotFound(ErrorResponse.BookNotFound);
        }

        // Will not be null thanks to Authorize attribute
        string username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Review? review = book.Reviews.FirstOrDefault(r => r.Username == username);

        int statusCode;
        if (review is not null)
        {
            review.StarRating = payload.StarRating;
            review.ReviewText = payload.ReviewText;
            statusCode = StatusCodes.Status200OK;
        }
        else
        {
            review = new(bookId, username, payload.StarRating, payload.ReviewText, DateTime.Now);
            statusCode = StatusCodes.Status201Created;
        }
        
        dbContext.Reviews.Update(review);
        
        book.UpdateStarRating();
        dbContext.Books.Update(book);
        dbContext.SaveChanges();

        var reviewOutput = dbContext.Reviews
            .Include(r => r.Reviewer)
            .First(r => r.Username == username && r.BookId == bookId);

        return StatusCode(statusCode, reviewOutput.ToResponse());
    }
    
    /// <summary>
    /// Removes the review by the logged-in user for the specified book 
    /// </summary>
    /// <param name="bookId">The book under which to delete a review</param>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="404">The book is not found, or the user has not reviewed the book</response>
    [HttpDelete]
    [Authorize]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete(string bookId)
    {
        Book? book = dbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .FirstOrDefault(b => b.BookId == bookId);
        if (book is null)
        {
            return NotFound(ErrorResponse.BookNotFound);
        }

        // Will not be null thanks to Authorize attribute
        string username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Review? review = book.Reviews.FirstOrDefault(r => r.Username == username);

        if (review is null)
        {
            return NotFound(ErrorResponse.ReviewNotFound);
        }
        
        dbContext.Remove(review);
        book.Reviews.Remove(review);  // manually detach from the Book, so .SaveChanges() doesn't have to be called twice
        
        book.UpdateStarRating();
        dbContext.Books.Update(book);
        dbContext.SaveChanges();
        
        return NoContent();
    }
}