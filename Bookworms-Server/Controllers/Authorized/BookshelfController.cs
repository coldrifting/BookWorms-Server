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
    /// Each bookshelf will have up to 3 books with their author, title and id for
    /// preview icon purposes.
    /// </summary>
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

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        List<BookshelfResponse> shelves = child.Bookshelves.Select(cb => cb.ToResponse(3)).ToList();
        return Ok(shelves);
    }
    
    /// <summary>
    /// Returns a bookshelves for the selected child under the logged in parent.
    /// The bookshelf preview will contain all books in the bookshelf.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to get</param>
    /// <returns>A bookshelf's name and all it's books</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or bookshelf does not exist</response>
    [HttpGet]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Details(string childId, string bookshelfName)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildBookshelf? childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        return Ok(childBookshelf.ToResponse());
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

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildBookshelf? childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is not null)
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

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildBookshelf? childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }
        
        ChildBookshelf? childBookshelf2 = child.Bookshelves.FirstOrDefault(cb => cb.Name == newName);
        if (childBookshelf2 is not null)
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
    public IActionResult Insert(string childId, string bookshelfName, [FromQuery] string bookId)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildBookshelf? childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }
        
        if (childBookshelf.BookshelfBooks.All(b => b.BookId != bookId))
        {
            Book? book = DbContext.Books.Find(bookId);
            if (book is null)
            {
                return UnprocessableEntity(ErrorResponse.BookIdInvalid);
            }
            
            childBookshelf.BookshelfBooks.Add(new(childBookshelf.BookshelfId, book.BookId));
            DbContext.SaveChanges();
        }
        
        return Details(childId, bookshelfName);
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
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildBookshelf? childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        ChildBookshelfBook? bookshelfBook = childBookshelf.BookshelfBooks.FirstOrDefault(b => b.BookId == bookId);
        if (bookshelfBook is null)
        {
            return NotFound(ErrorResponse.BookshelfBookNotFound);
        }

        DbContext.ChildBookshelfBooks.Remove(bookshelfBook);
        DbContext.SaveChanges();
        
        return Details(childId, bookshelfName);
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
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildBookshelf? childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        childBookshelf.BookshelfBooks.Clear();
        DbContext.SaveChanges();

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing bookshelf from the selected child under the logged in parent.
    /// </summary>
    /// <param name="childId">The ID of the child to use for this route</param>
    /// <param name="bookshelfName">The name of the bookshelf to delete</param>
    /// <returns>A list of all remaning bookshelves for the selected child</returns>
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

        Child? child = DbContext.Children
            .Include(child => child.Bookshelves)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
        
        if (child is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildBookshelf? childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        DbContext.ChildBookshelves.Remove(childBookshelf);
        DbContext.SaveChanges();

        return All(childId);
    }
}