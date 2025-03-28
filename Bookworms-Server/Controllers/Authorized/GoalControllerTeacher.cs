using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[Tags("Goals - Teachers")]
[Route("/homeroom/goals/{goalId}/[action]")]
public class GoalControllerTeacher(BookwormsDbContext context) : AuthControllerBase(context)
{
    [HttpGet]
    [Route("/homeroom/goals/all")]
    public IActionResult All()
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (DbContext.Classrooms.FirstOrDefault(c => c.TeacherUsername == teacher.Username) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        List<GoalResponse> classGoalsTeacher = DbContext.GoalClassesBase
            .Include(g => g.Logs)
            .Include(g => g.Classroom)
            .ThenInclude(c => c.Children)
            .Where(s => s.ClassCode == classroom.ClassroomCode)
            .Select(g => g.ToTeacherResponse(false))
            .ToList();

        return Ok(classGoalsTeacher);
    }

    [HttpPost]
    [Route("/homeroom/goals/add")]
    public IActionResult Add(GoalAddRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (DbContext.Classrooms.FirstOrDefault(c => c.TeacherUsername == teacher.Username) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        if (payload.GoalType is GoalType.Child)
        {
            return BadRequest(ErrorResponse.GoalTypeInvalid);
        }
        
        GoalClassBase newClassGoal = payload.GoalType == GoalType.Classroom
            ? new GoalClass(
                payload.GoalMetric,
                payload.Title,
                payload.StartDate,
                payload.EndDate,
                payload.Target,
                classroom.ClassroomCode)
            : new GoalClassAggregate(
                payload.GoalMetric,
                payload.Title,
                payload.StartDate,
                payload.EndDate,
                payload.Target,
                classroom.ClassroomCode);

        DbContext.GoalClassesBase.Add(newClassGoal);
        DbContext.SaveChanges();

        GoalClassBase returnGoal = DbContext.GoalClassesBase
            .Include(g => g.Classroom)
            .ThenInclude(c => c.Children)
            .Include(g => g.Logs)
            .First(g => g.GoalId == newClassGoal.GoalId);

        return Ok(returnGoal.ToTeacherResponse());
    }

    [HttpGet]
    public IActionResult Details(string goalId, [FromQuery] bool extended = false)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (DbContext.Classrooms.FirstOrDefault(c => c.TeacherUsername == teacher.Username) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }

        GoalClassBase? goalClass = DbContext.GoalClassesBase
            .Include(g => g.Logs)
            .Include(g => g.Classroom)
            .ThenInclude(c => c.Children)
            .FirstOrDefault(g => g.GoalId == goalId && g.ClassCode == classroom.ClassroomCode);
        
        if (goalClass is null)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        return Ok(goalClass.ToTeacherResponse(extended));
    }

    [HttpPut]
    public IActionResult Edit(string goalId, GoalEditRequest payload)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (DbContext.Classrooms.FirstOrDefault(c => c.TeacherUsername == teacher.Username) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }
        
        GoalClassBase? goalClass = DbContext.GoalClassesBase
            .Include(g => g.Logs)
            .Include(g => g.Classroom)
            .ThenInclude(c => c.Children)
            .FirstOrDefault(g => g.GoalId == goalId && g.ClassCode == classroom.ClassroomCode);
        
        if (goalClass is null)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }
        
        goalClass.GoalMetric = payload.GoalMetric ?? goalClass.GoalMetric;
        goalClass.Title = payload.Title ?? goalClass.Title;
        goalClass.StartDate = payload.StartDate ?? goalClass.StartDate;
        goalClass.EndDate = payload.EndDate ?? goalClass.EndDate;
        goalClass.Target = payload.Target ?? goalClass.Target;

        DbContext.SaveChanges();

        return Ok(goalClass.ToTeacherResponse());
    }

    [HttpDelete]
    public IActionResult Delete(string goalId)
    {
        if (CurrentUser is not Teacher teacher)
        {
            return Forbidden(ErrorResponse.UserNotTeacher);
        }

        if (DbContext.Classrooms.FirstOrDefault(c => c.TeacherUsername == teacher.Username) is not { } classroom)
        {
            return NotFound(ErrorResponse.ClassroomNotFound);
        }
        
        GoalClassBase? goalClass = DbContext.GoalClassesBase
            .FirstOrDefault(g => g.GoalId == goalId && g.ClassCode == classroom.ClassroomCode);
        
        if (goalClass is null)
        {
            return NotFound(ErrorResponse.GoalNotFound);
        }

        DbContext.GoalClassesBase.Remove(goalClass);
        DbContext.SaveChanges();

        return NoContent();
    }
}