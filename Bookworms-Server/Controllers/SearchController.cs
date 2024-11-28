using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
public class SearchController(BookwormsDbContext dbContext, IBookApiService bookApiService) : ControllerBase
{
    [HttpGet]
    [Route("/search/title")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookDto>))]
    public Task<IActionResult> ByName(string query)
    {
        List<Book> books = dbContext.Books
            .Where(b => b.Title.Contains(query))
            .ToList();
        
        Console.WriteLine(books.Count);
        
        List<BookDto> bookDtos = [];
        bookDtos.AddRange(
            from book in books
            let bookDetails = bookApiService.GetData(book.BookId).Result
            select BookDto.From(book, bookDetails)
        );

        return Task.FromResult<IActionResult>(Ok(bookDtos));
    }
}