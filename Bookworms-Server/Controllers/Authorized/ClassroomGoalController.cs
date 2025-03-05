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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalOverviewTeacherResponse))]
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
            ? Ok(classroom.Goals.ToTeacherResponse())
            : NotFound(ErrorResponse.ClassroomNotFound);
    }
    
    /// <summary>
    /// Adds a goal to the teacher's classroom.
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult AddNumBooks([FromBody] ClassGoalAddRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
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
        
        return Ok(goal.ToTeacherResponse());
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult DetailsBasic(string goalId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not { } goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        return Ok(goal.ToTeacherResponse());
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalDetailedTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult DetailsAll(string goalId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not { } goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        return Ok(goal.ToTeacherResponseFull());
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Edit(string goalId, [FromBody] ClassGoalEditRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not { } goal)
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

        return Ok(goal.ToTeacherResponse());
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassGoalOverviewTeacherResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete(string goalId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotTeacher);
        }

        if (GetClassroomRelations(teacher) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (GetClassGoal(classroom, goalId) is not { } goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        DbContext.ClassGoals.Remove(goal);
        DbContext.SaveChanges();

        return Ok(classroom.Goals.ToTeacherResponse());
    }

    // TODO - Add route(s) for viewing child classroom goals (As part of child goals?)
    


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

    private static ClassGoal? GetClassGoal(Classroom classroom, string goalId)
    {
        return classroom.Goals.FirstOrDefault(g => g.ClassGoalId == goalId);
    }
}