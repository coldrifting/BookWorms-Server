using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Children - Bookshelves")]
public class BookshelfController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Returns all bookshelves for the selected child under the logged in parent.
    /// </summary>
    /// <remarks>
    /// Each bookshelf will have up to 3 books with their author, title and id for
    /// preview icon purposes.
    /// </remarks>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <returns>A list of all bookshelves for a given child with preview book data</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child does not exist</response>
    [HttpGet]
    [Route("/children/{childId}/shelves")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult All(string childId)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        Child? child = GetChildWithAllBookshelves(childId, parent);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        List<BookshelfResponse> shelves = [
            child.Completed!.ToResponse(3),
            child.InProgress!.ToResponse(3)
        ];
        shelves.AddRange(child.Bookshelves.Select(cb => cb.ToResponse(3)).ToList());
        shelves.AddRange(child.Classrooms.SelectMany(
            c => c.Bookshelves.Select(cb => cb.ToResponse(3)).ToList()).ToList());
        return Ok(shelves);
    }
    
    /// <summary>
    /// Returns a bookshelves for the selected child under the logged in parent.
    /// </summary>
    /// <remarks>
    /// The bookshelf preview will contain all books in the bookshelf.
    /// </remarks>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfType">The type of the bookshelf to get</param>
    /// <param name="bookshelfName">The name of the bookshelf to get</param>
    /// <returns>A bookshelf's name and all it's books</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or bookshelf does not exist</response>
    [HttpGet]
    [Route("/children/{childId}/shelves/{bookshelfType}/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Details(string childId, BookshelfType bookshelfType, string bookshelfName)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        Child? child = GetChildWithSpecifiedBookshelves(childId, parent, bookshelfType, includeBooks:true);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        Bookshelf? bookshelf = GetBookshelfByName(child, bookshelfName, bookshelfType);
        if (bookshelf is null)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        return Ok(bookshelf.ToResponse());
    }

    /// <summary>
    /// Creates a new bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to create</param>
    /// <returns>A list of all bookshelves for the selected child</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or bookshelf does not exist</response>
    /// <response code="422">A bookshelf with the same name already exists for the selected child</response>
    [HttpPost]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult Add(string childId, string bookshelfName)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (bookshelfName is "Completed" or "In Progress")
        {
            return UnprocessableEntity(ErrorResponse.BookshelfNameReserved(bookshelfName));
        }

        if (GetChildWithSpecifiedBookshelves(childId, parent, BookshelfType.Custom) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetBookshelfByName(child, bookshelfName, BookshelfType.Custom) is ChildBookshelf)
        {
            return UnprocessableEntity(ErrorResponse.BookshelfAlreadyExists);
        }
        
        DbContext.ChildBookshelves.Add(new(bookshelfName, child.ChildId));
        DbContext.SaveChanges();
        
        return All(childId);
    }

    /// <summary>
    /// Renames an existing bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to rename</param>
    /// <param name="newName">The new name for the bookshelf</param>
    /// <returns>A list of all bookshelves for the selected child</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or bookshelf does not exist</response>
    /// <response code="422">A bookshelf with the new name already exists for this child</response>
    [HttpPost]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult Rename(string childId, string bookshelfName, [FromQuery] string newName)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithSpecifiedBookshelves(childId, parent, BookshelfType.Custom) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetBookshelfByName(child, bookshelfName, BookshelfType.Custom) is not ChildBookshelf childBookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }
        
        if (GetBookshelfByName(child, newName, BookshelfType.Custom) is ChildBookshelf)
        {
            return UnprocessableEntity(ErrorResponse.BookshelfAlreadyExists);
        }

        childBookshelf.Name = newName;
        DbContext.ChildBookshelves.Update(childBookshelf);
        DbContext.SaveChanges();
        
        return All(childId);
    }

    /// <summary>
    /// Inserts a book into a bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to insert a book into</param>
    /// <param name="bookId">The id of the book to insert</param>
    /// <param name="starRating">The child's star rating of the book, if inserting into Completed</param>
    /// <returns>The bookshelf name and a list of all the books it contains</returns>
    /// <response code="200">The book was inserted successfully</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child, bookshelf, or book does not exist</response>
    /// <response code="422">The book id is invalid</response>
    [HttpPut]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult Insert(string childId, string bookshelfName, [FromQuery] string bookId, [FromQuery] double? starRating = null)
    {
        BookshelfType bookshelfType = bookshelfName switch
        {
            "Completed" => BookshelfType.Completed,
            "In Progress" => BookshelfType.InProgress,
            _ => BookshelfType.Custom
        };

        // Okay to ignore star rating if it's provided in error, but not if it's not provided when needed
        if (bookshelfType is BookshelfType.Completed && starRating is null)
        {
            return UnprocessableEntity(ErrorResponse.StarRatingRequired);
        }
        
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithSpecifiedBookshelves(childId, parent, BookshelfType.Custom, includeBooks:true) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetBookshelfByName(child, bookshelfName, bookshelfType) is not { } bookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }
        
        if (bookshelf.Books.All(b => b.BookId != bookId))
        {
            if (DbContext.Books.Find(bookId) is not { } book)
            {
                return UnprocessableEntity(ErrorResponse.BookIdInvalid);
            }
            
            bookshelf.Books.Add(book);
            
            // Inserting into Completed needs a bit extra to happen
            if (bookshelfType == BookshelfType.Completed)
            {
                DbContext.CompletedBookshelfBooks.Add(new()
                {
                    BookshelfId = bookshelf.BookshelfId,
                    BookId = book.BookId,
                    StarRating = (double)starRating!
                });
                child.InProgress!.Books.Remove(book);
            }
            
            DbContext.SaveChanges();
        }
        
        return Details(childId, bookshelfType, bookshelfName);
    }

    /// <summary>
    /// Removes a book from a bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to remove a book from</param>
    /// <param name="bookId">The id of the book to remove</param>
    /// <returns>The bookshelf name and a list of all remaining books it contains</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or bookshelf does not exist, or the book id was not found in the bookshelf</response>
    [HttpDelete]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Remove(string childId, string bookshelfName, [FromQuery] string bookId)
    {
        BookshelfType bookshelfType = bookshelfName switch
        {
            "Completed" => BookshelfType.Completed,
            "In Progress" => BookshelfType.InProgress,
            _ => BookshelfType.Custom
        };

        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithSpecifiedBookshelves(childId, parent, bookshelfType, includeBooks:true) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetBookshelfByName(child, bookshelfName, bookshelfType) is not { } bookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }
        
        if (bookshelf.Books.All(b => b.BookId != bookId))
        {
            return NotFound(ErrorResponse.BookshelfBookNotFound);
        }
        
        Book book = DbContext.Books.Find(bookId)!;
        bookshelf.Books.Remove(book);
        DbContext.SaveChanges();
        
        return Details(childId, bookshelfType, bookshelfName);
    }

    /// <summary>
    /// Removes all books from a bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to clear</param>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or bookshelf does not exist</response>
    [HttpDelete]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Clear(string childId, string bookshelfName)
    {
        BookshelfType bookshelfType = bookshelfName switch
        {
            "Completed" => BookshelfType.Completed,
            "In Progress" => BookshelfType.InProgress,
            _ => BookshelfType.Custom
        };
        
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithSpecifiedBookshelves(childId, parent, bookshelfType, includeBooks:true) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetBookshelfByName(child, bookshelfName, bookshelfType) is not { } bookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        bookshelf.Books.Clear();
        DbContext.SaveChanges();

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing bookshelf from the selected child under the logged in parent.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to delete</param>
    /// <returns>A list of all remaining bookshelves for the selected child</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or bookshelf does not exist</response>
    [HttpDelete]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete(string childId, string bookshelfName)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithSpecifiedBookshelves(childId, parent, BookshelfType.Custom) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetBookshelfByName(child, bookshelfName, BookshelfType.Custom) is not ChildBookshelf childBookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        DbContext.ChildBookshelves.Remove(childBookshelf);
        DbContext.SaveChanges();

        return All(childId);
    }
    
    
    // Helpers

    private Child? GetChildWithAllBookshelves(string childId, Parent parent)
    {
        return this.DbContext.Children
            .Include(child => child.Completed)
            .ThenInclude(bookshelf => bookshelf!.Books)
            .Include(child => child.InProgress)
            .ThenInclude(bookshelf => bookshelf!.Books)
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .Include(child => child.Classrooms)
            .ThenInclude(classroom => classroom.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
    }

    private Child? GetChildWithSpecifiedBookshelves(
        string childId,
        Parent parent,
        BookshelfType bookshelfType,
        bool includeBooks = false)
    {
        IQueryable<Child> query = this.DbContext.Children;

        query = bookshelfType switch
        {
            BookshelfType.Completed => includeBooks
                ? query.Include(child => child.Completed!)
                    .ThenInclude(completed => completed.Books)
                : query.Include(child => child.Completed!),
            BookshelfType.InProgress => includeBooks
                ? query.Include(child => child.InProgress!)
                    .ThenInclude(completed => completed.Books)
                : query.Include(child => child.InProgress!),
            BookshelfType.Custom => includeBooks
                ? query.Include(child => child.Bookshelves)
                    .ThenInclude(completed => completed.Books)
                : query.Include(child => child.Bookshelves),
            BookshelfType.Classroom => includeBooks
                ? query.Include(child => child.Classrooms)
                    .ThenInclude(classroom => classroom.Bookshelves)
                    .ThenInclude(bookshelf => bookshelf.Books)
                : query.Include(child => child.Classrooms)
                    .ThenInclude(classroom => classroom.Bookshelves),
            _ => query
        };

        return query.FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
    }

    private static Bookshelf? GetBookshelfByName(Child child, string bookshelfName, BookshelfType bookshelfType)
    {
        return bookshelfType switch
        {
            BookshelfType.Completed => child.Completed,
            BookshelfType.InProgress => child.InProgress,
            BookshelfType.Custom => child.Bookshelves
                .FirstOrDefault(b => b.Name == bookshelfName),
            BookshelfType.Classroom => child.Classrooms
                .SelectMany(c => c.Bookshelves)
                .FirstOrDefault(cb => cb.Name == bookshelfName),
            _ => null
        };
    }
}