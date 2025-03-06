using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Classrooms - Teachers")]
public class ClassroomTeacherController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Gets the classroom details of the logged-in teacher
    /// </summary>
    /// <returns>The class details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a class</response>
    [HttpGet]
    [Route("/homeroom/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Details()
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        Classroom? classroom = GetClassroomRelations(teacher);

        return classroom is not null
            ? Ok(classroom.ToResponseTeacher())
            : NotFound(ErrorResponse.ClassroomNotFound);
    }
    
    /// <summary>
    /// Creates a new classroom for the logged-in teacher.
    /// </summary>
    /// <param name="className">The name of the new class</param>
    /// <returns>The new class details</returns>
    /// <response code="201">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="422">The teacher already has a classroom</response>
    [HttpPost]
    [Route("/homeroom/[action]")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    public IActionResult Create([FromQuery] string className)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroom(teacher) is not null)
        {
            return UnprocessableEntity(ErrorResponse.ClassroomAlreadyExists);
        }

        List<string> classCodes = DbContext.Classrooms.Select(c => c.ClassroomCode).ToList();
        string newClassCode = Classroom.GenerateClassroomCodeWithCheck(classCodes);
        
        // Manually set relations to avoid unnecessary DB join calls on new object
        Classroom newClass = new(teacher.Username, className, newClassCode)
        {
            Announcements = new List<ClassroomAnnouncement>(),
            Children = new List<Child>(),
            Bookshelves = new List<ClassroomBookshelf>()
        };
        teacher.Classroom = newClass;
        DbContext.SaveChanges();

        return Created($"/classrooms/{newClassCode}", newClass.ToResponseTeacher());
    }
    
    /// <summary>
    /// Renames the logged-in teacher's classroom
    /// </summary>
    /// <returns>The updated class details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a class</response>
    [HttpPut]
    [Route("/homeroom/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Rename([FromQuery] string newClassName)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }
        
        classroom.ClassroomName = newClassName;
        DbContext.SaveChanges();

        return Ok(classroom.ToResponseTeacher());
    }
    
    /// <summary>
    /// Changes the icon for the logged-in teacher's classroom
    /// </summary>
    /// <returns>The updated class details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a class</response>
    [HttpPut]
    [Route("/homeroom/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Icon([FromQuery] int newIcon)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }
        
        classroom.ClassIcon = newIcon;
        DbContext.SaveChanges();

        return Ok(classroom.ToResponseTeacher());
    }
    
    /// <summary>
    /// Deletes the logged-in teacher's classroom
    /// </summary>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a class</response>
    [HttpDelete]
    [Route("/homeroom/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete()
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroom(teacher) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }
        
        DbContext.Remove(classroom);
        DbContext.SaveChanges();
            
        return NoContent();
    }

    /// <summary>
    /// Creates a bookshelf in the teacher's classroom
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The class does not exist</response>
    /// <response code="422">A bookshelf with the same name already exists in this class</response>
    [HttpPost]
    [Route("/homeroom/shelves/{bookshelfName}/create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult BookshelfCreate(string bookshelfName)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Bookshelves.FirstOrDefault(c => c.Name == bookshelfName) is not null)
        {
            return UnprocessableEntity(ErrorResponse.BookshelfAlreadyExists);
        }

        ClassroomBookshelf bookshelf = new(bookshelfName, classroom.ClassroomCode);
        
        DbContext.Add(bookshelf);
        DbContext.SaveChanges();
            
        return Details();
    }

    /// <summary>
    /// Renames a bookshelf in the teacher's classroom
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The class or class bookshelf does not exist</response>
    /// <response code="422">A bookshelf with the same name already exists in this class</response>
    [HttpPost]
    [Route("/homeroom/shelves/{bookshelfName}/rename")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult BookshelfRename(string bookshelfName, [FromQuery] string newBookshelfName)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Bookshelves.FirstOrDefault(c => c.Name == bookshelfName) is not { } bookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        if (classroom.Bookshelves.FirstOrDefault(c => c.Name == newBookshelfName) is not null)
        {
            return UnprocessableEntity(ErrorResponse.BookshelfAlreadyExists);
        }

        bookshelf.Name = newBookshelfName;
        DbContext.SaveChanges();
            
        return Ok(classroom.ToResponseTeacher());
    }
    
    /// <summary>
    /// Inserts a book into the specified classroom bookshelf
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The class or class bookshelf does not exist</response>
    /// <response code="422">The book id is invalid</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    [Route("/homeroom/shelves/{bookshelfName}/[action]")]
    public IActionResult Insert(string bookshelfName, [FromQuery] string bookId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Bookshelves.FirstOrDefault(b => b.Name == bookshelfName) is not {} bookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        if (DbContext.Books.Find(bookId) is not { } book)
        {
            return UnprocessableEntity(ErrorResponse.BookIdInvalid);
        }

        bookshelf.Books.Add(book);
        DbContext.SaveChanges();
        
        return Ok(classroom.ToResponseTeacher());
    }

    /// <summary>
    /// Removes a book from the specified classroom bookshelf
    /// </summary>
    /// <returns>The updated class details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The class or class bookshelf does not exist, or the book was not found in the bookshelf</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [Route("/homeroom/shelves/{bookshelfName}/[action]")]
    public IActionResult Remove(string bookshelfName, [FromQuery] string bookId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Bookshelves.FirstOrDefault(b => b.Name == bookshelfName) is not { } bookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }

        if (bookshelf.Books.FirstOrDefault(b => b.BookId == bookId) is not {} book)
        {
            return NotFound(ErrorResponse.BookshelfBookNotFound);
        }
        
        bookshelf.Books.Remove(book);
        DbContext.SaveChanges();
        
        return Ok(classroom.ToResponseTeacher());
    }

    /// <summary>
    /// Deletes a bookshelf in the teacher's classroom
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The class or class bookshelf does not exist</response>
    [HttpDelete]
    [Route("/homeroom/shelves/{bookshelfName}/delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult BookshelfDelete(string bookshelfName)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Bookshelves.FirstOrDefault(c => c.Name == bookshelfName) is not {} bookshelf)
        {
            return NotFound(ErrorResponse.BookshelfNotFound);
        }
        
        DbContext.Remove(bookshelf);
        DbContext.SaveChanges();
            
        return Ok(classroom.ToResponseTeacher());
    }

    
    // Helper methods
    private Classroom? GetClassroom(Teacher teacher)
    {
        return DbContext.Classrooms
            .Include(classroom => classroom.Teacher)
            .FirstOrDefault(classroom => classroom.Teacher == teacher);
    }

    private Classroom? GetClassroomRelations(Teacher teacher)
    {
        return DbContext.Classrooms
            .Include(classroom => classroom.Teacher)
            .Include(classroom => classroom.Children)
            .Include(classroom => classroom.Announcements)
            .Include(classroom => classroom.Bookshelves)
            .ThenInclude(b => b.Books)
            .FirstOrDefault(classroom => classroom.Teacher == teacher);
    }
}