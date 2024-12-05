using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Search")]
public class SearchController(BookwormsDbContext dbContext, IBookApiService bookApiService) : ControllerBase
{
    /// <summary>
    /// Returns a list of books whose titles contain the given query string
    /// </summary>
    /// <param name="query">The string to look for in book titles</param>
    /// <returns>The list of books</returns>
    /// <response code="200">Returns the list of books</response>
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