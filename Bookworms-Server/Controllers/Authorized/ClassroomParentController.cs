using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Children - Classrooms")]
public class ClassroomParentController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Gets basic details about all classrooms the child has joined
    /// </summary>
    /// <returns>The class details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child does not exist or does not belong to the logged-in parent</response>
    [HttpGet]
    [Route("/children/{childId}/classrooms/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ClassroomChildResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult All(string childId)
    {
        if (CurrentUser is not Parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is not {} child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        List<ClassroomChildResponse> classes = DbContext.Classrooms
            .Include(c => c.Children)
            .Include(c => c.Bookshelves)
            .ThenInclude(b => b.Books)
            .Include(c => c.Teacher)
            .Where(c => c.Children.Contains(child))
            .Select(x => x.ToResponseChild())
            .ToList();

        return Ok(classes);
    }

    /// <summary>
    /// Adds the child to the classroom, if it exists
    /// </summary>
    /// <returns>The newly joined class details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child does not exist or does not belong to the logged-in parent,
    /// or the classroom does not exist</response>
    /// <response code="422">The child has already joined the classroom</response>
    [HttpPost]
    [Route("/children/{childId}/classrooms/{classCode}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomChildResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult Join(string childId, string classCode)
    {
        if (CurrentUser is not Parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is not {} child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetClassroomRelations(classCode) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Children.Contains(child))
        {
            return UnprocessableEntity(ErrorResponse.ChildAlreadyInClass);
        }

        classroom.Children.Add(child);
        DbContext.SaveChanges();

        return Ok(classroom.ToResponseChild());
    }

    /// <summary>
    /// Removes the child from the classroom, if the child is in that classroom
    /// </summary>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child does not exist or does not belong to the logged-in parent,
    /// or the classroom does not exist, or the child has not joined the classroom</response>
    [HttpDelete]
    [Route("/children/{childId}/classrooms/{classCode}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Leave(string childId, string classCode)
    {
        if (CurrentUser is not Parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is not {} child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetClassroom(classCode) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (!classroom.Children.Contains(child))
        {
            return NotFound(ErrorResponse.ChildNotInClass);
        }

        classroom.Children.Remove(child);
        DbContext.SaveChanges();

        return NoContent();
    }

    // Helper methods
    private Classroom? GetClassroom(string classCode)
    {
        return DbContext.Classrooms
            .Include(classroom => classroom.Children)
            .FirstOrDefault(classroom => classroom.ClassroomCode == classCode);
    }

    private Classroom? GetClassroomRelations(string classCode)
    {
        return DbContext.Classrooms
            .Include(classroom => classroom.Teacher)
            .Include(classroom => classroom.Children)
            .Include(classroom => classroom.Bookshelves)
            .ThenInclude(bookshelf => bookshelf.Books)
            .FirstOrDefault(c => c.ClassroomCode == classCode);
    }
}