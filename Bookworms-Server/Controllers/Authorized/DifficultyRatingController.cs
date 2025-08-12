using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Book Difficulty Ratings")]
public class DifficultyRatingController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Adds the given child's rating for the given book to the book's difficulty rating history.
    /// </summary>
    /// <remarks>
    /// If the child is not leveled or the book's level has been frozen, the child's level will be updated;
    /// otherwise the book's level will updated, unless neither are leveled in which case no action will be taken.
    /// </remarks>
    /// <returns>Info about the updated entity and its updated level</returns>
    /// <response code="200">Success. The rating update the book's or child's level</response>
    /// <response code="204">Success. The rating had no effect</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The book or the child was not found</response>
    [HttpPost]
    [Route("/books/{bookId}/rate-difficulty")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdatedLevelResponse))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult Add(string bookId, DifficultyRatingAddRequest ratingRequest)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(ratingRequest.ChildId) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }
        
        Book? book = DbContext.Books
            .Include(b => b.DifficultyRatings)
            .FirstOrDefault(b => b.BookId == bookId);
        if (book is null)
        {
            return NotFound(ErrorResponse.BookNotFound);
        }

        if (book.DifficultyRatings.Any(d => d.ChildId == ratingRequest.ChildId))
        {
            return UnprocessableEntity(ErrorResponse.DuplicateDifficultyRating);
        }
        
        
        if (child.ReadingLevel == null)
        {
            // If neither the book nor the child have a reading level, no one can be updated
            if (book.Level == null)
            {
                return NoContent();
            }

            // But if the book does have a reading level, use that to set the child's
            child.ReadingLevel = book.Level + (ratingRequest.Rating - 3) * 3;
            DbContext.SaveChanges();
            return Ok(child.ToUpdatedLevelResponse(null));
        }
        
        // If the book's level has been locked in, use it to adjust the child's 
        if (book.LevelIsLocked)
        {
            int? oldChildLevel = child.ReadingLevel;
            child.AdjustReadingLevel(book, ratingRequest.Rating);
            DbContext.SaveChanges();
            return Ok(child.ToUpdatedLevelResponse(oldChildLevel));
        }

        // Otherwise, create the new rating and tell the book to update its level
        DifficultyRating difficultyRating = new(bookId, child.ChildId, child.ReadingLevel.Value, ratingRequest.Rating);
        DbContext.DifficultyRatings.Add(difficultyRating);
        int? oldBookLevel = book.Level;
        book.UpdateLevel(DbContext);
        DbContext.SaveChanges();
        return Ok(book.ToUpdatedLevelResponse(oldBookLevel));
    }
}