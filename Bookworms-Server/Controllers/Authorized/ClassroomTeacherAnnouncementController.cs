using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Classrooms - Teachers - Announcements")]
public class ClassAnnouncementController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Gets a list of all announcements for this teacher's classroom
    /// </summary>
    /// <returns>A list of annoucements</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not set up a classroom</response>
    [HttpGet]
    [Route("/homeroom/announcements/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ClassroomAnnouncementResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult All()
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        return Ok(classroom.Announcements.Select(a => a.ToResponse()));
    }
    
    /// <summary>
    /// Creates a new annoucement for this teacher's classroom
    /// </summary>
    /// <returns>The newly created annoucement details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not set up a classroom</response>
    [HttpPut]
    [Route("/homeroom/announcements/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomAnnouncementResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Add([FromBody] ClassroomAnnouncementAddRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        ClassroomAnnouncement announcement = new(classroom.ClassroomCode, payload.Title, payload.Body, DateTime.Now);
        classroom.Announcements.Add(announcement);
        DbContext.SaveChanges();

        return Ok(announcement.ToResponse());
    }
    
    /// <summary>
    /// Edits an annoucement in this teacher's classroom
    /// </summary>
    /// <returns>The edited annoucement details</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not set up a classroom,
    /// the announcement id is invalid, or does not belong to this teachers class</response>
    [HttpPut]
    [Route("/homeroom/announcements/{announcementId}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassroomAnnouncementResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Edit(string announcementId, [FromBody] ClassroomAnnouncementEditRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Announcements.FirstOrDefault(a => a.AnnouncementId == announcementId) is not { } announcement)
        {
            return NotFound(ErrorResponse.ClassroomAnnouncementNotFound);
        }

        announcement.Title = payload.Title ?? announcement.Title;
        announcement.Body = payload.Body ?? announcement.Body;
        
        DbContext.SaveChanges();

        return Ok(announcement.ToResponse());
    }
    
    /// <summary>
    /// Deletes an annoucement in this teacher's classroom
    /// </summary>
    /// <returns>The edited annoucement details</returns>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not set up a classroom,
    /// the announcement id is invalid, or does not belong to this teachers class</response>
    [HttpDelete]
    [Route("/homeroom/announcements/{announcementId}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete(string announcementId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Announcements.FirstOrDefault(a => a.AnnouncementId == announcementId) is not { } announcement)
        {
            return NotFound(ErrorResponse.ClassroomAnnouncementNotFound);
        }

        DbContext.ClassroomAnnouncements.Remove(announcement);
        DbContext.SaveChanges();

        return NoContent();
    }
    
    /// <summary>
    /// Deletes all annoucements in this teacher's classroom
    /// </summary>
    /// <returns>The edited annoucement details</returns>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not set up a classroom,
    /// the announcement id is invalid, or does not belong to this teachers class</response>
    [HttpDelete]
    [Route("/homeroom/announcements/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Clear()
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        classroom.Announcements.Clear();
        DbContext.SaveChanges();

        return NoContent();
    }
    
    // Helper methods
    
    private Classroom? GetClassroomRelations(Teacher teacher)
    {
        return DbContext.Classrooms
            .Include(classroom => classroom.Announcements)
            .FirstOrDefault(classroom => classroom.TeacherUsername == teacher.Username);
    }
}