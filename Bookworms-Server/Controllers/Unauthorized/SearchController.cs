using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Search")]
[Produces("application/json")]
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookResponse>))]
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
        
        IQueryable<Book> q;

        if (query is not null)
        {
            // 1. Select using the match score thing. Keep only those with scores greater than 0.5, and order by score
            // 2. Select using literal text match against title and author. Include those if the first query returned nothing
            q = dbContext.Books.FromSql($"""
                WITH
                    ScoredBooks AS (
                        SELECT Books.*,
                               MATCH(Title, Description, Subjects, Authors) AGAINST({query} IN NATURAL LANGUAGE MODE) AS score
                        FROM Books
                        HAVING score > 0.5
                        ORDER BY score DESC
                    )
                    
                SELECT BookId, Title, Authors, Description, Subjects, Isbn10, Isbn13, CoverId, PageCount, PublishYear, Level, LevelIsLocked, StarRating, TimeAdded, SimilarBooks
                FROM ScoredBooks
                
                UNION
                
                SELECT *
                FROM Books
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ScoredBooks
                ) 
                AND Title LIKE CONCAT('%', {query}, '%')
                OR Authors LIKE CONCAT('%', {query}, '%')
                """);
        }
        else
        {
            q = dbContext.Books.AsQueryable();
            
            if (title is not null)
            {
                q = q.Where(b => EF.Functions.Like(b.Title, $"%{title}%"));
            }

            if (author is not null)
            {
                q = q.Where(b => EF.Functions.Like((string)(object)b.Authors, $"%{author}%"));
            }
        }
        
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
        
        if (subjects.Count > 0)
        {
            // Interpolated string in EF/LINQ lambda is not supported, so we have to use plain old concatenation
            q = q.Where(b => subjects.Any(subject => EF.Functions.Like((string)(object)b.Subjects, "%" + subject + "%")));
        }
        
        List<BookResponse> bookResponseList = q
            .Take(maxBooksToReturn)
            .Select(book => book.ToResponse())
            .ToList();
        
        return Task.FromResult<IActionResult>(Ok(bookResponseList));
    }
}