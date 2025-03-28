using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Goals - Child View")]
[Route("/children/{childId}/goals/{goalId}/[action]")]
public class GoalControllerChild(BookwormsDbContext context) : AuthControllerBase(context)
{
    [HttpGet]
    [Route("/children/{childId}/goals/[action]")]
    public IActionResult All(string childId)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        List<GoalResponse> childGoals = DbContext.GoalChildren
            .Where(g => g.ChildId == childId)
            .Select(g => g.ToResponse())
            .ToList();

        List<GoalResponse> classGoalsChild = GetClassGoalsChild(childId)
            .Select(g => g.ToChildResponse(childId))
            .ToList();

        List<GoalResponse> allGoals = childGoals.Concat(classGoalsChild).ToList();

        return Ok(allGoals);
    }


    [HttpPost]
    [Route("/children/{childId}/goals/[action]")]
    public IActionResult Add(string childId, GoalAddRequest payload)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (payload.GoalType is GoalType.Classroom or GoalType.ClassroomAggregate)
        {
            return BadRequest(ErrorResponse.GoalTypeInvalid);
        }

        var newChildGoal = new GoalChild(
            payload.GoalMetric,
            payload.Title,
            payload.StartDate,
            payload.EndDate,
            payload.Target,
            childId);

        DbContext.GoalChildren.Add(newChildGoal);
        DbContext.SaveChanges();

        return Ok(newChildGoal.ToResponse());
    }

    [HttpGet]
    public IActionResult Details(string childId, string goalId)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (DbContext.GoalChildren.Find(goalId) is not { } goal || goal.ChildId != childId)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        return Ok(goal.ToResponse());
    }
    
    [HttpPut]
    public IActionResult Log(string childId, string goalId, int progress)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (DbContext.Goals.FirstOrDefault(g => g.GoalId == goalId) is not { } goal)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        if (goal is GoalChild goalChild)
        {
            if (goalChild.ChildId != childId)
            {
                return NotFound(ErrorResponse.GoalNotFound);
            }

            goalChild.Progress = progress;
            
            DbContext.SaveChanges();
            return Ok(goalChild.IsCompleted);
        }

        if (GetClassGoalWithLogs(goalId, childId) is not {} classGoalWithLogs)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        if (classGoalWithLogs.Logs.FirstOrDefault(l => l.ChildId == childId) is { } log)
        {
            log.Progress = progress;
        }
        else
        {
            DbContext.GoalClassLogs.Add(new(goalId, childId, classGoalWithLogs.ClassCode, progress));
        }
        
        DbContext.SaveChanges();
        
        GoalClassLog log2 = DbContext.GoalClassLogs.Include(l => l.GoalClassBase).First(l => l.ChildId == childId && l.GoalId == goalId);

        return Ok(log2.IsCompleted);
    }
    
    [HttpPut]
    public IActionResult Edit(string childId, string goalId, GoalEditRequest payload)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (DbContext.GoalChildren.FirstOrDefault(g => g.GoalId == goalId && g.ChildId == childId) is not { } goalChild)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        goalChild.GoalMetric = payload.GoalMetric ?? goalChild.GoalMetric;
        goalChild.Title = payload.Title ?? goalChild.Title;
        goalChild.StartDate = payload.StartDate ?? goalChild.StartDate;
        goalChild.EndDate = payload.EndDate ?? goalChild.EndDate;
        goalChild.Target = payload.Target ?? goalChild.Target;
        
        DbContext.SaveChanges();
        
        return Ok(goalChild.ToResponse());
    }
    
    [HttpDelete]
    public IActionResult Delete(string childId, string goalId)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is null)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }

        if (DbContext.GoalChildren.FirstOrDefault(g => g.GoalId == goalId && g.ChildId == childId) is not { } goalChild)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        DbContext.GoalChildren.Remove(goalChild);
        DbContext.SaveChanges();
        
        return NoContent();
    }
    
    // Helpers

    private List<GoalClassBase> GetClassGoalsChild(string childId)
    {
        return DbContext.GoalClassesBase
            .Include(g => g.Logs)
            .Include(g => g.Classroom)
            .ThenInclude(c => c.Children)
            .Where(s => s.Classroom.Children.Any(c => c.ChildId == childId)).ToList();
    }

    private GoalClassBase? GetClassGoalWithLogs(string goalId, string childId)
    {
        return DbContext.GoalClassesBase
            .Include(g => g.Classroom)
            .ThenInclude(c => c.Children)
            .Include(g => g.Logs)
            .FirstOrDefault(g => g.GoalId == goalId && g.Classroom.Children.Any(c => c.ChildId == childId));
    }
}