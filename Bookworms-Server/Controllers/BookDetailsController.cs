using System.IO.Compression;
using System.Text.Json.Nodes;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Book Details")]
public class BookDetailsController(BookwormsDbContext dbContext, IBookApiService bookApiService) : ControllerBase
{
    /// <summary>
    /// Returns details about the given book
    /// </summary>
    /// <param name="bookId">The Google Books ID of the book to target</param>
    /// <returns>A BookDetailsDTO object</returns>
    /// <response code="200">Returns the book details</response>
    /// <response code="404">If the specified book is not found</response>
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
        {
            return NotFound(ErrorDTO.BookNotFound);
        }

        JsonObject bookDetailsJson = await bookApiService.GetData(bookId);
        var details = BookDetailsDTO.From(bookEntity, bookDetailsJson);
        return Ok(details);
    }

    /// <summary>
    /// Returns the cover image for the specified book
    /// </summary>
    /// <param name="bookId">The Google Books ID of the book to target</param>
    /// <returns>A JPEG file</returns>
    /// <response code="200">Returns the cover image</response>
    /// <response code="404">If the specified book is not found</response>
    [HttpGet]
    [Route("/books/{bookId}/cover")]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status200OK, "image/jpeg", Type = typeof(File))]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status404NotFound, "application/json", Type = typeof(ErrorDTO))]
    public async Task<IActionResult> Image(string bookId)
    {
        try
        {
            byte[] b = await bookApiService.GetImage(bookId);
            return File(b, "image/jpeg");
        }
        catch (HttpRequestException)
        {
            return NotFound(ErrorDTO.BookCoverNotFound);
        }
    }

    /// <summary>
    /// Returns the cover images for the books specified in the request body
    /// </summary>
    /// <param name="bookIds">The Google Books IDs of the books to target</param>
    /// <returns>A zip archive of JPEG files</returns>
    /// <response code="200">Returns the zip archive of cover images</response>
    [HttpPost]
    [Route("/books/covers")]
    [ProducesResponseType(typeof(Task<IActionResult>), StatusCodes.Status200OK, "application/zip", Type = typeof(File))]
    [SwaggerRequestExample(typeof(List<string>), typeof(ImagesRequestBodyExample))]
    public async Task<IActionResult> Images([FromBody] List<string> bookIds)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            foreach (var bookId in bookIds)
            {
                var zipEntry = archive.CreateEntry($"{bookId}_cover.jpg", CompressionLevel.Optimal);
                byte[] imageBytes = await bookApiService.GetImage(bookId);

                await using var zipStream = zipEntry.Open();
                await zipStream.WriteAsync(imageBytes);
            }
        }
        
        return File(ms.ToArray(), "application/zip", "CoverImages.zip");
    }
}