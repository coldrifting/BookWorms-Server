using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Book Details")]
public class BookDetailsController(BookwormsDbContext dbContext, IBookApiService bookApiService) : ControllerBase
{
    [HttpGet]
    [Route("/books/{bookId}/details")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookDetailsDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Get(string bookId)
    {
        Book? bookEntity = dbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .SingleOrDefault(b => b.BookId == bookId);
        
        if (bookEntity == null)
            return NotFound();

        string bookDataJson = bookApiService.GetData(bookId);
        if (string.IsNullOrWhiteSpace(bookDataJson))
            return NotFound();
        
        JObject bookDetailsJson = JObject.Parse(bookDataJson);
        var details = BookDetailsDTO.From(bookEntity, bookDetailsJson);
        return Ok(details);
    }
}