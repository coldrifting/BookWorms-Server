using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Children - Bookshelves")]
public class BookshelfController(BookwormsDbContext dbContext) : ControllerBase
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
    [Authorize]
    [Route("/children/{childId}/shelves")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult All(string childId)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book)
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }
        
        List<BookshelfPreviewResponseDTO> bookshelves = [];
        bookshelves.AddRange(child.Bookshelves.Select(cb =>
            BookshelfPreviewResponseDTO.From(cb.Name, cb.BookshelfBooks.Take(3).Select(c => c.Book)!)));
        return Ok(bookshelves);
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
    [Authorize]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfPreviewResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Details(string childId, string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book)
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        var bookshelf = dbContext.Bookshelves
            .Include(b => b.Books)
            .Include(bookshelf => bookshelf.BookshelfBooks)
            .First(b => b.BookshelfId == childBookshelf.BookshelfId);
        
        return Ok(BookshelfPreviewResponseDTO.From(bookshelf.Name, bookshelf.BookshelfBooks));
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
    [Authorize]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Add(string childId, string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book).Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is not null)
        {
            return UnprocessableEntity(ErrorDTO.BookshelfAlreadyExists);
        }
        
        dbContext.Bookshelves.Add(new ChildBookshelf(bookshelfName, child.ChildId));
        dbContext.SaveChanges();
        
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
    [Authorize]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Rename(string childId, string bookshelfName, [FromQuery] string newName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book).Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }
        
        var childBookshelf2 = child.Bookshelves.FirstOrDefault(cb => cb.Name == newName);
        if (childBookshelf2 is not null)
        {
            return UnprocessableEntity(ErrorDTO.BookshelfAlreadyExists);
        }

        childBookshelf.Name = newName;
        dbContext.ChildBookshelves.Update(childBookshelf);
        dbContext.SaveChanges();
        
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
    [Authorize]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfPreviewResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Insert(string childId, string bookshelfName, [FromQuery] string bookId)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book)
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }
        
        if (childBookshelf.BookshelfBooks.All(b => b.BookId != bookId))
        {
            Book? book = dbContext.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book is null)
            {
                return UnprocessableEntity(ErrorDTO.BookIdInvalid);
            }
            
            childBookshelf.BookshelfBooks.Add(new(childBookshelf.BookshelfId, book.BookId));
            dbContext.SaveChanges();
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
    [Authorize]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfPreviewResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Remove(string childId, string bookshelfName, [FromQuery] string bookId)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book)
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        BookshelfBook? bookshelfBook = childBookshelf.BookshelfBooks.FirstOrDefault(b => b.BookId == bookId);
        if (bookshelfBook is null)
        {
            return NotFound(ErrorDTO.BookshelfBookNotFound);
        }

        dbContext.BookshelfBooks.Remove(bookshelfBook);
        dbContext.SaveChanges();
        
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
    [Authorize]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Clear(string childId, string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book).Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        childBookshelf.BookshelfBooks.Clear();
        dbContext.SaveChanges();

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
    [Authorize]
    [Route("/children/{childId}/shelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Delete(string childId, string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        Parent? parent = dbContext.Parents.FirstOrDefault(p => p.Username == parentUsername);
        if (parent is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children
            .Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book).Include(child => child.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parentUsername);
        if (child is null)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var childBookshelf = child.Bookshelves.FirstOrDefault(cb => cb.Name == bookshelfName);
        if (childBookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        dbContext.ChildBookshelves.Remove(childBookshelf);
        dbContext.SaveChanges();

        return All(childId);
    }
}