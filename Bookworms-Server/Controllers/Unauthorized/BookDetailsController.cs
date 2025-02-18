using System.IO.Compression;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using BookwormsServer.Swagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Book Details")]
public class BookDetailsController(BookwormsDbContext dbContext, IBookApiService bookApiService) : ControllerBase
{
    /// <summary>
    /// Returns basic details about a book
    /// </summary>
    /// <param name="bookId">The Book ID of the book to target</param>
    /// <returns>Basic details about the requested book</returns>
    /// <response code="200">Success</response>
    /// <response code="404">The book is not found</response>
    [HttpGet]
    [ResponseCache(Duration = 60)]
    [Route("/books/{bookId}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Details(string bookId)
    {
        Book? bookEntity = dbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .SingleOrDefault(b => b.BookId == bookId);

        return bookEntity == null 
            ? NotFound(ErrorResponse.BookNotFound)
            : Ok(bookEntity.ToResponse());
    }
    /// <summary>
    /// Returns extended details about a book
    /// </summary>
    /// <param name="bookId">The Book ID of the book to target</param>
    /// <returns>All details about the requested book</returns>
    /// <response code="200">Success</response>
    /// <response code="404">The book is not found</response>
    [HttpGet]
    [ResponseCache(Duration = 60)]
    [Route("/books/{bookId}/details/all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookResponseExtended))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult All(string bookId)
    {
        Book? bookEntity = dbContext.Books
            .Include(b => b.Reviews)
            .ThenInclude(r => r.Reviewer)
            .SingleOrDefault(b => b.BookId == bookId);

        return bookEntity == null ? 
            NotFound(ErrorResponse.BookNotFound) : 
            Ok(bookEntity.ToResponseExtended());
    }

    /// <summary>
    /// Gets the cover image for a book
    /// </summary>
    /// <param name="bookId">The Book ID of the book to target</param>
    /// <returns>A JPEG file</returns>
    /// <response code="200">Success</response>
    /// <response code="404">The book is not found</response>
    [HttpGet]
    [Route("/books/{bookId}/[action]")]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status200OK, "image/jpeg", Type = typeof(File))]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status404NotFound, "application/json", Type = typeof(ErrorResponse))]
    public async Task<IActionResult> Cover(string bookId)
    {
        string? coverId = dbContext.Books
            .SingleOrDefault(b => b.BookId == bookId)
            ?.CoverId
            ?.ToString();
        if (coverId == null)
        {
            return NotFound(ErrorResponse.BookCoverNotFound);
        }
        
        try
        {
            byte[] b = await bookApiService.GetImage(coverId);
            return File(b, "image/jpeg");
        }
        catch (HttpRequestException)
        {
            return NotFound(ErrorResponse.BookCoverNotFound);
        }
    }

    /// <summary>
    /// Gets the cover images for a list of books
    /// </summary>
    /// <param name="bookIds">The Book IDs of the books to target</param>
    /// <returns>A zip archive of book cover images</returns>
    /// <response code="200">Success</response>
    [HttpPost]
    [Route("/books/[action]")]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status200OK, "application/zip", Type = typeof(File))]
    [SwaggerRequestExample(typeof(List<string>), typeof(SwaggerExamples.ImagesRequestBodyExample))]
    public async Task<IActionResult> Covers([FromBody] List<string> bookIds)
    {
        List<Book> books = dbContext.Books
            .Where(b => bookIds.Contains(b.BookId))
            .ToList();
        
        List<(string bookId, string coverId)> bookStubs = bookIds
            .Select(id => books.FirstOrDefault(b => b.BookId == id))
            .Select(b => (bookId: b?.BookId, coverId: b?.CoverId?.ToString()))
            .Where(t => t is { bookId: not null, coverId: not null })
            .ToList()!;
        
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var stub in bookStubs)
            {
                var zipEntry = archive.CreateEntry($"{stub.bookId}_cover.jpg", CompressionLevel.Optimal);
                byte[] imageBytes = await bookApiService.GetImage(stub.coverId);

                await using var zipStream = zipEntry.Open();
                await zipStream.WriteAsync(imageBytes);
            }
        }
        
        return File(ms.ToArray(), "application/zip", "CoverImages.zip");
    }
}