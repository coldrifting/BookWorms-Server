using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Reviews")]
public class ReviewsController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Gets reviews for the specified book
    /// </summary>
    /// <param name="bookId">The Google Books ID of the book to target</param>
    /// <param name="start" default="0">The start index from which to start returning reviews (first is 0)</param>
    /// <param name="max" default="-1">The maximum number of reviews to return (use -1 for unconstrained)</param>
    /// <returns>The list of reviews</returns>
    /// <response code="200">Returns the list of requested reviews</response>
    /// <response code="404">If the specified book is not found</response>
    [HttpGet]
    [Route("/books/{bookId}/reviews")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ReviewDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Get(string bookId, int start, int max)
    {
        Book? book = dbContext.Books
            .Include(book => book.Reviews)
            .FirstOrDefault(b => b.BookId == bookId);

        if (book is null)
        {
            return NotFound(ErrorDTO.BookNotFound);
        }
        
        List<ReviewDTO> output = [];

        List<Review> x = dbContext.Reviews
            .Include(review => review.Reviewer)
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.ReviewDate).ToList();

        for (int i = 0; i < x.Count; i++)
        {
            if (i >= start)
            {
                output.Add(ReviewDTO.From(x[i]));
            }

            if (max > 0 && i >= start + max - 1)
            {
                break;
            }
        }
            
        return Ok(output);
    }
    
    /// <summary>
    /// Adds/updates the review for the specified book and user
    /// </summary>
    /// <param name="bookId">The Google Books ID of the book to target</param>
    /// <param name="reviewDto">The data with which to populate the new/updated review</param>
    /// <returns>The newly created or updated review</returns>
    /// <response code="200">Returns the updated review</response>
    /// <response code="201">Returns the newly created review</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="404">If the specified book is not found</response>
    [HttpPut]
    [Authorize]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDTO))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult AddOrUpdate(string bookId, [FromBody] ReviewAddOrUpdateRequestDTO reviewDto)
    {
        if (!dbContext.Books.Any(b => b.BookId == bookId))
        {
            return NotFound(ErrorDTO.BookNotFound);
        }

        // Will not be null thanks to Authorize attribute
        string username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Review? review = dbContext.Reviews
            .Include(r => r.Reviewer)
            .FirstOrDefault(r => r.BookId == bookId && 
                                 r.Reviewer!.Username == username);

        int statusCode;
        if (review is not null)
        {
            review.StarRating = reviewDto.StarRating;
            review.ReviewText = reviewDto.ReviewText;
            statusCode = 200;
        }
        else
        {
            review = new(bookId, username, reviewDto.StarRating, reviewDto.ReviewText, DateTime.Now);
            statusCode = 201;
        }

        dbContext.Reviews.Update(review);
        dbContext.SaveChanges();
        
        var rx = dbContext.Reviews
            .Where(r => r.ReviewId == review.ReviewId)
            .Include(r => r.Reviewer)
            .FirstOrDefault();

        return StatusCode(statusCode, ReviewDTO.From(rx!));
    }
    
    /// <summary>
    /// Removes the review by the logged in user for the specified book 
    /// </summary>
    /// <param name="bookId">The Google Books ID of the book to target</param>
    /// <response code="204">If the review was removed successfully</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="404">If the specified book is not found, or the user has not left a review for the book</response>
    [HttpDelete]
    [Authorize]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Delete(string bookId)
    {
        // Will not be null thanks to Authorize attribute
        string username = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        Review? review = dbContext.Reviews
            .Include(r => r.Reviewer)
            .FirstOrDefault(r => r.BookId == bookId && 
                                        r.Reviewer!.Username == username);

        if (review is null)
        {
            return NotFound(ErrorDTO.BookNotFound);
        }

        dbContext.Remove(review);
        dbContext.SaveChanges();

        Response.Headers.Append("location", "");
        
        return NoContent();
    }
}