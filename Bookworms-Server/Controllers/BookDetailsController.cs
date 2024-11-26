using System.Text.Json.Nodes;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IActionResult> Get(string bookId)
    {
        Book? bookEntity = dbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .SingleOrDefault(b => b.BookId == bookId);
        
        if (bookEntity == null)
            return NotFound();
        
        JsonObject bookDetailsJson = await bookApiService.GetData(bookId);
        var details = BookDetailsDTO.From(bookEntity, bookDetailsJson);
        return Ok(details);
    }

    [HttpGet]
    [Route("/books/{bookId}/cover")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(File))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public async Task<IActionResult> Image(string bookId)
    {
        byte[] b = await bookApiService.GetImage(bookId);
        return File(b, "image/jpeg");
    }
}