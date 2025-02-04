using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Search")]
public class SearchController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Returns a list of books matching a query string.
    /// If the query parameter is passed, title and author are ignored.
    /// </summary>
    /// <param name="query">The string to look for in book titles or authors</param>
    /// <param name="title">The string to look for in book titles</param>>
    /// <param name="author">The string to look for in book authors</param>
    /// <param name="subjects">A list of subjects that the book must contain at least one of</param>
    /// <param name="ratingMin">The minimum review rating the book must have</param>
    /// <param name="levelMin">The minimum difficulty level that the book must be in</param>
    /// <param name="levelMax">The maximum difficulty level that the book must be in</param>
    /// <returns>A list of matching books</returns>
    /// <response code="200">Success</response>
    [HttpGet]
    [Route("/search")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookDTO>))]
    public Task<IActionResult> ByName(
        [FromQuery] string? query,
        [FromQuery] string? title,
        [FromQuery] string? author,
        [FromQuery] List<string> subjects,
        [FromQuery] double? ratingMin,
        [FromQuery] int? levelMin,
        [FromQuery] int? levelMax
        )
    {
        const int maxBooksToReturn = 30;
        
        IQueryable<Book> q = dbContext.Books.AsQueryable();

        if (ratingMin is not null)
        {
            q = q.Where(b => b.StarRating >= ratingMin);
        }

        if (levelMin is not null)
        {
            q = q.Where(b => b.Level >= levelMin);
        }

        if (levelMax is not null)
        {
            q = q.Where(b => b.Level <= levelMax);
        }

        if (query is not null)
        {
            q = q.Where(b => EF.Functions.Like(b.Title, $"%{query}%") ||
                             EF.Functions.Like((string)(object)b.Authors, $"%{query}%"));
        }
        else
        {
            if (title is not null)
            {
                q = q.Where(b => EF.Functions.Like(b.Title, $"%{title}%"));
            }

            if (author is not null)
            {
                q = q.Where(b => EF.Functions.Like((string)(object)b.Authors, $"%{author}%"));
            }
        }

        
        if (subjects.Count > 0)
        {
            // Interpolated string in EF/LINQ lambda is not supported, so we have to use plain old concatenation
            q = q.Where(b => subjects.Any(subject => EF.Functions.Like((string)(object)b.Subjects, "%" + subject + "%")));
        }

        List<Book> books = q.ToList();
        
        List<BookDTO> bookDTOList = books.Select(BookDTO.From)
            .Take(maxBooksToReturn)
            .ToList();
        
        return Task.FromResult<IActionResult>(Ok(bookDTOList));
    }
}