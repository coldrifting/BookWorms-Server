using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Goals - Classrooms")]
public class ClassroomGoalController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Gets a list of a basic goals information for all goals in the class
    /// </summary>
    /// <returns>A list of basic goal info</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a class</response>
    [HttpGet]
    [Route("/homeroom/goals")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalOverviewResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult All()
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }
        
        Classroom? classroom = DbContext.Classrooms
            .Include(classroom => classroom.Teacher)
            .Include(classroom => classroom.Children)
            .Include(classroom => classroom.Goals)
            .ThenInclude(classGoal => classGoal.GoalLogs)
            .ThenInclude(classGoalLog => classGoalLog.ClassroomChild)
            .ThenInclude(classroomChild => classroomChild.Child)
            .FirstOrDefault(c => c.Teacher == teacher);

        return classroom is not null
            ? Ok(classroom.Goals.ToResponse())
            : NotFound(ErrorResponse.ClassroomNotFound);
    }
    
    /// <summary>
    /// Adds a goal with a number of books read target to the teacher classroom.
    /// Specify a target number of books to read for a number of books goal,
    /// otherwise leave it out for a completion goal.
    /// </summary>
    /// <returns>A list of basic goal info</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a class</response>
    [HttpPost]
    [Route("/homeroom/goals/add")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult AddNumBooks([FromBody] ClassGoalAddRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        ClassGoal goal = payload.TargetNumBooks is null
            ? new ClassGoalCompletion(
                classroom.ClassroomCode,
                payload.Title,
                DateOnly.FromDateTime(DateTime.Today),
                payload.EndDate)
            : new ClassGoalNumBooks(
                classroom.ClassroomCode,
                payload.Title,
                DateOnly.FromDateTime(DateTime.Today),
                payload.EndDate,
                payload.TargetNumBooks.Value);

        DbContext.ClassGoals.Add(goal);
        DbContext.SaveChanges();

        // Since there would be no logs anyway, avoid re-querying the DB by providing fake data
        goal.GoalLogs = new List<ClassGoalLog>();
        
        return Ok(goal.ToResponse());
    }
    
    
    /// <summary>
    /// Gets basic details for a class goal 
    /// </summary>
    /// <returns>Detailed goal info with student completion status</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a classroom,
    /// the goal is not part of this teachers class, or does not exist</response>
    [HttpGet]
    [Route("/homeroom/goals/{goalId}/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult DetailsBasic(string goalId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not {} goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        return Ok(goal.ToResponse());
    }
    
    
    /// <summary>
    /// Gets student completion details for a class goal 
    /// </summary>
    /// <returns>Detailed goal info with student completion status</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a classroom,
    /// the goal is not part of this teachers class, or does not exist</response>
    [HttpGet]
    [Route("/homeroom/goals/{goalId}/details/all")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalDetailedResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult DetailsAll(string goalId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not {} goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        return Ok(goal.ToResponseFull());
    }

    /// <summary>
    /// Edits an existing classroom goal
    /// </summary>
    /// <returns>A list of basic goal info</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a classroom,
    /// the goal is not part of this teachers class, or does not exist</response>
    [HttpPut]
    [Route("/homeroom/goals/{goalId}/edit")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Edit(string goalId, [FromBody] ClassGoalEditRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not {} goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        goal.Title = payload.NewTitle ?? goal.Title;
        goal.EndDate = payload.NewEndDate ?? goal.EndDate;

        if (goal is ClassGoalNumBooks goalNumBooks)
        {
            goalNumBooks.TargetNumBooks = payload.NewTargetNumBooks ?? goalNumBooks.TargetNumBooks;
        }

        //DbContext.ClassGoals.Add(goal);
        DbContext.SaveChanges();

        return Ok(goal.ToResponse());
    }
    
    
    /// <summary>
    /// Removes a goal from the class
    /// </summary>
    /// <returns>A list of basic goal info</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a teacher</response>
    /// <response code="404">The teacher has not created a classroom,
    /// the goal is not part of this teachers class, or does not exist</response>
    [HttpDelete]
    [Route("/homeroom/goals/{goalId}/delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalOverviewResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete(string goalId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not {} classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not {} goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        DbContext.ClassGoals.Remove(goal);
        DbContext.SaveChanges();

        return Ok(classroom.Goals.ToResponse());
    }

    private Classroom? GetClassroomRelations(Teacher teacher)
    {
        return DbContext.Classrooms
            .Include(classroom => classroom.Teacher)
            .Include(classroom => classroom.Children)
            .Include(classroom => classroom.Goals)
            .ThenInclude(classGoal => classGoal.GoalLogs)
            .ThenInclude(classGoalLog => classGoalLog.ClassroomChild)
            .ThenInclude(classroomChild => classroomChild.Child)
            .FirstOrDefault(c => c.Teacher == teacher);
    }

    private ClassGoal? GetClassGoal(Classroom classroom, string goalId)
    {
        return classroom.Goals.FirstOrDefault(g => g.ClassGoalId == goalId);
    }
    
    /// <summary>
    /// Adds or Replaces a child's logged progress towards a classroom goal
    /// </summary>
    /// <returns>True if the child has completed the goal</returns>
    /// <response code="200">Success</response>
    /// <response code="400">The request was missing required information, or had invalid 0 values</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The class or goal does not exist, or the child has not joined the class</response>
    [HttpPut]
    [Route("/children/{childId}/classrooms/{classCode}/goals/{goalId}/update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult UpdateClassGoal(string childId, string classCode, string goalId, [FromBody] ClassGoalLogEditRequest payload)
    {
        if (CurrentUser is not Parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }
        
        if (CurrentUserChild(childId) is not { })
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (GetClassroom(classCode) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (classroom.Children.FirstOrDefault(c => c.ChildId == childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotInClass);
        }

        if (classroom.Goals.FirstOrDefault(g => g.ClassGoalId == goalId) is not { } goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        if (payload.ToClassGoalLog(goal, childId) is not {} log)
        {
            return BadRequest(ErrorResponse.GoalEditInfoMissing);
        }

        if (log is ClassGoalLogNumBooks && payload.NumBooks is 0 || 
            log is ClassGoalLogCompletion && (payload.Progress is 0 || payload.Duration is 0))
        {
            return BadRequest(ErrorResponse.GoalEditInfoInvalid);
        }

        if (goal.GoalLogs.FirstOrDefault(l => l.ChildId == childId && l.ClassGoalId == goal.ClassGoalId) is {} existingLog)
        {
            DbContext.Entry(existingLog).CurrentValues.SetValues(log);
        }
        else
        {
            DbContext.ClassGoalLogs.Add(log);
        }

        DbContext.SaveChanges();

        // Coerce the existing goal for this method call
        log.ClassGoal = goal;
        return Ok(log.IsGoalCompleted);
    }

    // TODO - Add route(s) for viewing child classroom goals (As part of child goals?)
    
    private Classroom? GetClassroom(string classCode)
    {
        return DbContext.Classrooms
            .Include(c => c.Children)
            .Include(c => c.Goals)
            .ThenInclude(g => g.GoalLogs)
            .FirstOrDefault(c => c.ClassroomCode == classCode);
    }
}