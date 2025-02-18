using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Book Reviews - Authorized")]
public class ReviewUpdateController(BookwormsDbContext context) : AuthControllerBase(context)
{
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
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewResponse))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult AddOrUpdate(string bookId, [FromBody] ReviewEditRequest payload)
    {
        Book? book = DbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .FirstOrDefault(b => b.BookId == bookId);
        if (book is null)
        {
            return NotFound(ErrorResponse.BookNotFound);
        }
        
        Review? review = book.Reviews.FirstOrDefault(r => r.Username == CurrentUser.Username);

        int statusCode;
        if (review is not null)
        {
            review.StarRating = payload.StarRating;
            review.ReviewText = payload.ReviewText;
            statusCode = StatusCodes.Status200OK;
        }
        else
        {
            review = new(bookId, CurrentUser.Username, payload.StarRating, payload.ReviewText, DateTime.Now);
            statusCode = StatusCodes.Status201Created;
        }
        
        DbContext.Reviews.Update(review);
        
        book.UpdateStarRating();
        DbContext.Books.Update(book);
        DbContext.SaveChanges();

        Review reviewOutput = DbContext.Reviews
            .Include(r => r.Reviewer)
            .First(r => r.Username == CurrentUser.Username && r.BookId == bookId);

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
        Book? book = DbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .FirstOrDefault(b => b.BookId == bookId);
        if (book is null)
        {
            return NotFound(ErrorResponse.BookNotFound);
        }
        
        Review? review = book.Reviews.FirstOrDefault(r => r.Username == CurrentUser.Username);
        if (review is null)
        {
            return NotFound(ErrorResponse.ReviewNotFound);
        }
        
        DbContext.Remove(review);
        book.Reviews.Remove(review);  // manually detach from the Book, so .SaveChanges() doesn't have to be called twice
        
        book.UpdateStarRating();
        DbContext.Books.Update(book);
        DbContext.SaveChanges();
        
        return NoContent();
    }
}