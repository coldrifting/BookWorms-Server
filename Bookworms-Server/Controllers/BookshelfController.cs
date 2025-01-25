using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Bookshelves")]
public class BookshelfController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Returns all bookshelves for the selected child under the logged in parent.
    /// Each bookshelf will have up to 3 books with their author, title and id for
    /// preview icon purposes.
    /// </summary>
    /// <returns>A list of all bookshelves for the selected child with preview book data</returns>
    /// <response code="200">A list of bookshelves for the selected child was retrieved successfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    [HttpGet]
    [Authorize]
    [Route("/bookshelves/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    public IActionResult All()
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        Child child = dbContext.Children
            .Include(c => c.Bookshelves).ThenInclude(cb => cb.BookshelfBooks)
            .ThenInclude(bookshelfBook => bookshelfBook.Book)
            .First(c => c.ChildId == parent.SelectedChild.ChildId);
        
        List<BookshelfPreviewResponseDTO> bookshelves = [];
        bookshelves.AddRange(child.Bookshelves.Select(cb => BookshelfPreviewResponseDTO.From(cb.Name, cb.BookshelfBooks.Take(3).Select(c => c.Book)!)));
        return Ok(bookshelves);
    }
    
    /// <summary>
    /// Returns a bookshelves for the selected child under the logged in parent.
    /// The bookshelf preview will contain all books in the bookshelf.
    /// </summary>
    /// <param name="bookshelfName">The name of the bookshelf to get</param>
    /// <returns>A bookshelf with its name and a list of all its books</returns>
    /// <response code="200">The bookshelf was found succesfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If the bookshelf does not exist</response>
    [HttpGet]
    [Authorize]
    [Route("/bookshelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfPreviewResponseDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Details(string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        if (dbContext.Children
                .Include(c => c.Bookshelves).ThenInclude(cb => cb.Books).Include(child => child.Bookshelves)
                .ThenInclude(bookshelf => bookshelf.BookshelfBooks).ThenInclude(bookshelfBook => bookshelfBook.Book)
                .First(c => c.ChildId == parent.SelectedChild.ChildId).Bookshelves
                .FirstOrDefault(cb => cb.Name == bookshelfName) is not
            { } childBookshelf)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        IEnumerable<Book> books = childBookshelf.BookshelfBooks.Select(c => c.Book)!;
        return Ok(BookshelfPreviewResponseDTO.From(childBookshelf.Name, books));
    }

    /// <summary>
    /// Creates a new bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="bookshelfName">The name of the bookshelf to create</param>
    /// <returns>A list of all bookshelves for the selected child</returns>
    /// <response code="200">The bookshelf was created successfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="422">If a bookshelf with the same name already exists for the selected child</response>
    [HttpPost]
    [Authorize]
    [Route("/bookshelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Add(string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        Child child = parent.SelectedChild;

        if (dbContext.Children
            .Include(c => c.Bookshelves)
            .First(c => c.ChildId == child.ChildId).Bookshelves
            .Any(cb => cb.Name == bookshelfName))
        {
            return UnprocessableEntity(ErrorDTO.BookshelfAlreadyExists);
        }

        dbContext.Bookshelves.Add(new ChildBookshelf(bookshelfName, child.ChildId));
        dbContext.SaveChanges();

        return All();
    }

    /// <summary>
    /// Renames an existing bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="bookshelfName">The name of the bookshelf to rename</param>
    /// <param name="newName">The new name for the bookshelf</param>
    /// <returns>A list of all bookshelves for the selected child</returns>
    /// <response code="200">The bookshelf was renamed successfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no bookshelf exists that matches the given name</response>
    /// <response code="422">If a bookshelf with the same name already exists for the selected child</response>
    [HttpPost]
    [Authorize]
    [Route("/bookshelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Rename(string bookshelfName, [FromQuery] string newName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        Child child = parent.SelectedChild;

        ChildBookshelf? bookshelf = dbContext.Children
            .Include(c => c.Bookshelves)
            .First(c => c.ChildId == child.ChildId).Bookshelves
            .FirstOrDefault(cb => cb.Name == bookshelfName);

        if (bookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }
        
        if (dbContext.Children
            .Include(c => c.Bookshelves)
            .First(c => c.ChildId == child.ChildId).Bookshelves
            .Any(cb => cb.Name == newName))
        {
            return UnprocessableEntity(ErrorDTO.BookshelfAlreadyExists);
        }

        bookshelf.Name = newName;
        dbContext.SaveChanges();

        return All();
    }

    /// <summary>
    /// Inserts a book into a bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="bookshelfName">The name of the bookshelf to insert a book into</param>
    /// <param name="bookId">The id of the book to insert</param>
    /// <returns>The bookshelf name and a list of all the books it contains</returns>
    /// <response code="200">The book was inserted successfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no bookshelf exists that matches the given name</response>
    /// <response code="422">If no book matching the given book id exists</response>
    [HttpPut]
    [Authorize]
    [Route("/bookshelves/{bookshelfName}/[action]/{bookId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfPreviewResponseDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Insert(string bookshelfName, string bookId)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        Child child = parent.SelectedChild;

        ChildBookshelf? bookshelf = dbContext.Children
            .Include(c => c.Bookshelves)
            .ThenInclude(cb => cb.BookshelfBooks)
            .First(c => c.ChildId == child.ChildId).Bookshelves
            .FirstOrDefault(cb => cb.Name == bookshelfName);

        if (bookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        if (bookshelf.BookshelfBooks.All(b => b.BookId != bookId))
        {
            Book? book = dbContext.Books.FirstOrDefault(b => b.BookId == bookId);
            if (book is null)
            {
                return UnprocessableEntity(ErrorDTO.BookNotFound);
            }
            
            bookshelf.BookshelfBooks.Add(new(bookshelf.BookshelfId, book.BookId));
            dbContext.SaveChanges();
        }
        
        return Details(bookshelfName);
    }

    /// <summary>
    /// Removes a book from a bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="bookshelfName">The name of the bookshelf to remove a book from</param>
    /// <param name="bookId">The id of the book to remove</param>
    /// <returns>The bookshelf name and a list of all remaining books it contains</returns>
    /// <response code="200">The book was removed from the bookshelf successfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no bookshelf exists that matches the given name, or the book id was not found in the bookshelf</response>
    [HttpDelete]
    [Authorize]
    [Route("/bookshelves/{bookshelfName}/[action]/{bookId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookshelfPreviewResponseDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Remove(string bookshelfName, string bookId)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        Child child = parent.SelectedChild;
        
        ChildBookshelf? bookshelf = dbContext.Children
            .Include(c => c.Bookshelves)
            .ThenInclude(cb => cb.BookshelfBooks)
            .First(c => c.ChildId == child.ChildId).Bookshelves
            .FirstOrDefault(cb => cb.Name == bookshelfName);

        if (bookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }
        
        var book = bookshelf.BookshelfBooks.FirstOrDefault(b => b.BookId == bookId);
        if (book is null)
        {
            return NotFound(ErrorDTO.BookshelfBookNotFound);
        }

        bookshelf.BookshelfBooks.Remove(book);
        dbContext.SaveChanges();

        return Details(bookshelfName);
    }

    /// <summary>
    /// Removes all books from a bookshelf for the selected child under the logged in parent.
    /// </summary>
    /// <param name="bookshelfName">The name of the bookshelf to clear</param>
    /// <response code="204">The bookshelf was cleared successfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no bookshelf exists that matches the given name</response>
    [HttpDelete]
    [Authorize]
    [Route("/bookshelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Clear(string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        Child child = parent.SelectedChild;
        
        ChildBookshelf? bookshelf = dbContext.Children
            .Include(c => c.Bookshelves).ThenInclude(bookshelf => bookshelf.BookshelfBooks)
            .First(c => c.ChildId == child.ChildId).Bookshelves
            .FirstOrDefault(cb => cb.Name == bookshelfName);

        if (bookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        bookshelf.BookshelfBooks.Clear();
        dbContext.SaveChanges();

        return NoContent();
    }

    /// <summary>
    /// Deletes an existing bookshelf from the selected child under the logged in parent.
    /// </summary>
    /// <param name="bookshelfName">The name of the bookshelf to delete</param>
    /// <returns>A list of all remaning bookshelves for the selected child</returns>
    /// <response code="200">The bookshelf was deleted successfully</response>
    /// <response code="400">If no child is selected, or the user has no children</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no bookshelf exists that matches the given name</response>
    [HttpDelete]
    [Authorize]
    [Route("/bookshelves/{bookshelfName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookshelfPreviewResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Delete(string bookshelfName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        if (parent.SelectedChild is null)
        {
            return BadRequest(ErrorDTO.NoChildSelected);
        }

        Child child = parent.SelectedChild;

        ChildBookshelf? bookshelf = dbContext.Children
            .Include(c => c.Bookshelves)
            .First(c => c.ChildId == child.ChildId).Bookshelves
            .FirstOrDefault(cb => cb.Name == bookshelfName);

        if (bookshelf is null)
        {
            return NotFound(ErrorDTO.BookshelfNotFound);
        }

        dbContext.Bookshelves.Remove(bookshelf);
        dbContext.SaveChanges();

        return All();
    }
}