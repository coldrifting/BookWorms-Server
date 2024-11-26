using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Reviews")]
public class ReviewsController(BookwormsDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ReviewDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorDTO))]
    public IActionResult Add(string bookId, string username, [FromBody] ReviewRequestDTO reviewDto)
    {
        if (!dbContext.Books.Any(b => b.BookId == bookId) ||
            !dbContext.Users.Any(u => u.Username == username))
        {
            return BadRequest(new ErrorDTO(
                "Invalid book or user id", 
                "Review could not be created as the book or user id does not exist"));
        }
        
        Review? testExists = dbContext.Reviews.Include(r => r.Reviewer)
            .FirstOrDefault(r => r.BookId == bookId && r.Reviewer!.Username == username);
        
        if (testExists is not null)
        {
            return Conflict(new ErrorDTO("Can not create review", "Review already exists"));
        }

        Review review = new(bookId, username, reviewDto.StarRating, reviewDto.ReviewText, DateTime.Now);
        dbContext.Reviews.Add(review);
        dbContext.SaveChanges();

        var rx = dbContext.Reviews.Where(r => r.ReviewId == review.ReviewId).Include(r => r.Reviewer).FirstOrDefault();

        return Created($"reviews/get/{review.ReviewId}", ReviewDTO.From(rx));
    }
    
    // TODO - Infer username when this becomes an authorized route
    [HttpPut]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Review))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Update(string bookId, string username, [FromBody] ReviewRequestDTO reviewDto)
    {
        Review? review = dbContext.Reviews.Include(r => r.Reviewer)
            .FirstOrDefault(r => r.BookId == bookId && r.Reviewer!.Username == username);

        if (review is null)
        {
            return BadRequest(new ErrorDTO(
                "Invalid book or user id", 
                "Review could not be updated as the book or user id does not exist"));
        }

        review.StarRating = reviewDto.StarRating;
        review.ReviewText = reviewDto.ReviewText;
        
        dbContext.SaveChanges();

        return Ok();
    }
    
    // TODO - Infer username when this becomes an authorized route
    [HttpDelete]
    [Route("/books/{bookId}/review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Delete(string bookId, string username)
    {
        Review? review = dbContext.Reviews.Include(r => r.Reviewer)
            .FirstOrDefault(r => r.BookId == bookId && r.Reviewer!.Username == username);

        if (review is null)
        {
            return BadRequest(new ErrorDTO(
                "Invalid book or user id", 
                "Review could not be deleted as the book or user id does not exist"));
        }

        dbContext.Remove(review);
        
        dbContext.SaveChanges();

        return Ok();
    }

    [HttpGet]
    [Route("/books/{bookId}/reviews")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ReviewDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Book(string bookId, int start, int max)
    {
        Book? book = dbContext.Books
            .Include(book => book.Reviews)
            .FirstOrDefault(b => b.BookId == bookId);
        
        if (book is not null)
        {
            var output = new List<ReviewDTO>();

            var x = dbContext.Reviews
                .Include(review => review.Reviewer)
                .Where(r => r.BookId == bookId).ToList();

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

        return BadRequest(new ErrorDTO("Invalid reviewId", "Could not find a review matching the given id"));
    }

    [HttpGet]
    [Route("/reviews/{reviewId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReviewDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Get(string reviewId)
    {
        Review? review = dbContext.Reviews.Include(review => review.Reviewer).FirstOrDefault(r => r.ReviewId.ToString() == reviewId);
        if (review is not null)
        {
            return Ok(ReviewDTO.From(review));
        }

        return BadRequest(new ErrorDTO("Invalid reviewId", "Could not find a review matching the given id"));
    }
}