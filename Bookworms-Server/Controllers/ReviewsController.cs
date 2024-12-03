using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Reviews")]
public class ReviewsController(BookwormsDbContext dbContext) : ControllerBase
{
    // TODO - Infer username when this becomes an authorized route
    [HttpPut]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDTO))]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult AddOrUpdate(string bookId, string username, [FromBody] ReviewAddOrUpdateRequestDTO reviewDto)
    {
        if (!dbContext.Books.Any(b => b.BookId == bookId))
        {
            return NotFound(ErrorDTO.BookNotFound);
        } 
        
        // TODO - Replace with username inference
        if (!dbContext.Users.Any(u => u.Username == username))
        {
            return NotFound(ErrorDTO.UsernameNotFound);
        }

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
    
    // TODO - Infer username when this becomes an authorized route
    [HttpDelete]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Delete(string bookId, string username)
    {
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
        
        return Ok();
    }

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
}