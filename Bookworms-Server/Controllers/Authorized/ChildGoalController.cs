using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Goals - Children")]
public class ChildGoalController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Gets a list of a basic information for all goals for the child
    /// </summary>
    /// <remarks>
    /// This includes goals for any classrooms to which the child belongs.
    /// </remarks>
    /// <returns>A list of goal info</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child was not found</response>
    [HttpGet]
    [Route("/children/{childId}/goals")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AllGoalOverviewResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult All(string childId)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }
        
        if (GetChildWithAllGoals(childId, parent) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        return Ok(child.ToGoalsResponse());
    }
    
    /// <summary>
    /// Adds a goal to the specified child.
    /// Specify a target number of books to read for a number of books goal,
    /// otherwise leave it out for a completion goal.
    /// </summary>
    /// <returns>Info for the added goal</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child was not found</response>
    [HttpPost]
    [Route("/children/{childId}/goals/add")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenericGoalChildResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Add(string childId, [FromBody] ChildGoalAddRequest payload)
    {
        if (CurrentUser is not Parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        ChildGoal childGoal = payload.TargetNumBooks is null
            ? new ChildGoalCompletion(
                childId,
                payload.Title,
                payload.StartDate,
                payload.EndDate)
            : new ChildGoalNumBooks(
                childId,
                payload.Title,
                payload.StartDate,
                payload.EndDate,
                payload.TargetNumBooks.Value);

        DbContext.ChildGoals.Add(childGoal);
        DbContext.SaveChanges();
        
        return Ok(childGoal.ToChildResponse());
    }
    
    
    /// <summary>
    /// Gets basic details for a goal
    /// </summary>
    /// <remarks>
    /// This also works for goals belonging to any classrooms to which the child belongs.
    /// </remarks>
    /// <returns>Info for the specified goal</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or goal was not found</response>
    [HttpGet]
    [Route("/children/{childId}/goals/{goalId}/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenericGoalChildResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Details(string childId, string goalId)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithAllGoals(childId, parent) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        
        if (GetChildGoal(child, goalId) is { } childGoal)
        {
            return Ok(childGoal.ToChildResponse());
        }

        if (GetClassGoal(child, goalId) is { } classGoal)
        {
            return Ok(classGoal.ToChildResponse(childId));
        }
        
        return NotFound(ErrorResponse.GoalNotFound);
    }

    /// <summary>
    /// Edits an existing goal of the child
    /// </summary>
    /// <returns>Info for the edited goal</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or goal was not found</response>
    [HttpPut]
    [Route("/children/{childId}/goals/{goalId}/edit")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenericGoalChildResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Edit(string childId, string goalId, [FromBody] ClassGoalEditRequest payload)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithAllGoals(childId, parent) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }
    
        if (GetChildGoal(child, goalId) is not { } goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
    
        goal.Title = payload.NewTitle ?? goal.Title;
        goal.StartDate = payload.NewStartDate ?? goal.StartDate;
        goal.EndDate = payload.NewEndDate ?? goal.EndDate;
    
        if (goal is ChildGoalNumBooks goalNumBooks)
        {
            goalNumBooks.TargetNumBooks = payload.NewTargetNumBooks ?? goalNumBooks.TargetNumBooks;
        }
    
        DbContext.SaveChanges();
    
        return Ok(goal.ToChildResponse());
    }
    
    /// <summary>
    /// Removes a goal from the child
    /// </summary>
    /// <returns>A list of goal info for the goals that remain</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or goal was not found</response>
    [HttpDelete]
    [Route("/children/{childId}/goals/{goalId}/delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AllGoalOverviewResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete(string childId, string goalId)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithAllGoals(childId, parent) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }
    
        if (GetChildGoal(child, goalId) is not { } goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
    
        DbContext.ChildGoals.Remove(goal);
        DbContext.SaveChanges();
    
        return Ok(child.ToGoalsResponse());
    }
    
    
    /// <summary>
    /// Updates a child's progress towards a goal
    /// </summary>
    /// <returns>True if the child has completed the goal</returns>
    /// <response code="200">Success</response>
    /// <response code="400">The request was missing required information, or had invalid 0 values</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child or goal was not found</response>
    [HttpPut]
    [Route("/children/{childId}/goals/{goalId}/updateProgress")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult UpdateGoalProgress(string childId, string goalId, [FromBody] GoalProgressUpdateRequest payload)
    {
        if (CurrentUser is not Parent parent)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
        }

        if (GetChildWithAllGoals(childId, parent) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }
        
        
        // Child and class goals require handling that's distinct enough to merit relegating the logic to helpers
        
        if (GetChildGoal(child, goalId) is { } childGoal)
        {
            return UpdateChildGoalProgress(childGoal, payload);
        }

        if (GetClassGoal(child, goalId) is { } classGoal)
        {
            return UpdateClassGoalProgress(classGoal, payload, childId);
        }
        
        return NotFound(ErrorResponse.GoalNotFound);
    }


    private IActionResult UpdateChildGoalProgress(ChildGoal childGoal, GoalProgressUpdateRequest payload)
    {
        switch (childGoal)
        {
            case ChildGoalNumBooks numBooksGoal:
                if (payload.NumBooks is null)
                    return BadRequest(ErrorResponse.GoalEditInfoMissing);
                if (payload.NumBooks is 0)
                    return BadRequest(ErrorResponse.GoalEditInfoInvalid);
                numBooksGoal.NumBooks = (int)payload.NumBooks!;
                break;
            case ChildGoalCompletion completionGoal:
                if (payload.Progress is null || payload.Duration is null)
                    return BadRequest(ErrorResponse.GoalEditInfoMissing);
                if (payload.Progress is 0 || payload.Duration is 0)
                    return BadRequest(ErrorResponse.GoalEditInfoInvalid);
                completionGoal.Progress = (float)payload.Progress!;
                completionGoal.Duration = (int)payload.Duration!;
                break;
            default:
                throw new ArgumentException();
        }
        
        DbContext.SaveChanges();
        return Ok(childGoal.IsCompleted);
    }

    private IActionResult UpdateClassGoalProgress(ClassGoal classGoal, GoalProgressUpdateRequest payload, string childId)
    {
        if (payload.ToClassGoalLog(classGoal, childId) is not { } log)
        {
            return BadRequest(ErrorResponse.GoalEditInfoMissing);
        }
        
        if (log is ClassGoalLogNumBooks && payload.NumBooks is 0 || 
            log is ClassGoalLogCompletion && (payload.Progress is 0 || payload.Duration is 0))
        {
            return BadRequest(ErrorResponse.GoalEditInfoInvalid);
        }

        if (classGoal.GoalLogs.FirstOrDefault(
                l => l.ChildId == childId && l.ClassGoalId == classGoal.ClassGoalId) is { } existingLog)
        {
            DbContext.Entry(existingLog).CurrentValues.SetValues(log);
        }
        else
        {
            DbContext.ClassGoalLogs.Add(log);
        }

        DbContext.SaveChanges();

        // Coerce the existing goal for this method call
        log.ClassGoal = classGoal;
        return Ok(log.IsGoalCompleted);
    }

    private Child? GetChildWithAllGoals(string childId, Parent parent)
    {
        return DbContext.Children
            .Include(child => child.Goals)
            .Include(child => child.Classrooms)
            .ThenInclude(classroom => classroom.Goals)
            .ThenInclude(goal => goal.GoalLogs)
            .FirstOrDefault(c => c.ChildId == childId && c.ParentUsername == parent.Username);
    }

    private static ChildGoal? GetChildGoal(Child child, string goalId)
    {
        return child.Goals.FirstOrDefault(g => g.ChildGoalId == goalId);
    }

    private static ClassGoal? GetClassGoal(Child child, string goalId)
    {
        return child.Classrooms.SelectMany(c => c.Goals).FirstOrDefault(g => g.ClassGoalId == goalId);
    }
}