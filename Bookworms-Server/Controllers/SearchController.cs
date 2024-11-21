using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(AllBookwormsDbContext dbContext, IBookApiService bookApiService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Search(string query)
    {
        List<Book> books = dbContext.Books
            .Where(b => b.Title.Contains(query))
            .ToList();
        
        Console.WriteLine(books.Count);
        
        List<BookDto> bookDtos = [];
        bookDtos.AddRange(
            from book in books
            let bookDetails = JObject.Parse(bookApiService.GetData(book.BookId))
            select BookDto.From(book, bookDetails)
        );

        return Ok(bookDtos);
    }
}