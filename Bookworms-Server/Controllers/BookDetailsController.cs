using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("BookDetails")]
[Route("[controller]")]
public class BookDetailsController(AllBookwormsDbContext dbContext, IBookApiService bookApiService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<BookDetailsDTO> Get(string bookId)
    {
        Book bookEntity = dbContext.Books
            .Include(b => b.Reviews).ThenInclude(r => r.Reviewer)
            .Single(b => b.BookId == bookId);
        
        JObject bookDetailsJson = JObject.Parse(bookApiService.GetData(bookId));
        var details = BookDetailsDTO.From(bookEntity, bookDetailsJson);
        return Ok(details);
    }
}