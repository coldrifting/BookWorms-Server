using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Route("[controller]")]
public class ReviewsController(AllBookwormsDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public void Add(string bookId, [FromBody] ReviewDTO reviewDto)
    {
        Review review = new Review(bookId, reviewDto.ReviewerUsername, reviewDto.StarRating, reviewDto.ReviewText);
        dbContext.Reviews.Add(review);
        dbContext.SaveChanges();
    }
}