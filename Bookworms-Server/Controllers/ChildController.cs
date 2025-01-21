using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Accounts - Children")]
[Route("user/children/[action]")]
public class ChildController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Adds a child under the logged-in parent with the specified name
    /// </summary>
    [HttpPost]
    [Authorize]
    public IActionResult Add(string childName)
    {
        // This should never be null thanks to the authorize attribute above
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        IQueryable<Child> childMatch = GetChildByName(parentUsername, childName);

        if (childMatch.Any())
        {
            return BadRequest(ErrorDTO.ChildAlreadyExists);
        }

        Child child = new(childName, parentUsername);
        dbContext.Children.Add(child);
        dbContext.SaveChanges();
        
        return Ok();
    }

    
    /// <summary>
    /// Edits properties of a given child under the logged-in parent
    /// </summary>
    [HttpPost]
    [Authorize]
    public IActionResult Edit(string childName, ChildEditDTO payload)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        IQueryable<Child> childMatch = GetChildByName(parentUsername, childName);

        if (!childMatch.Any())
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        var child = childMatch.First();
        if (payload.DateOfBirth is not null)
        {
            child.DateOfBirth = payload.DateOfBirth;
        }

        if (payload.ClassroomCode is not null)
        {
            child.ClassroomCode = payload.ClassroomCode;
        }

        if (payload.ReadingLevel is not null)
        {
            child.ReadingLevel = payload.ReadingLevel;
        }

        if (payload.NewName is not null)
        {
            IQueryable<Child> childMatchNew = GetChildByName(parentUsername, payload.NewName);
            if (childMatchNew.Any())
            {
                return BadRequest(ErrorDTO.ChildAlreadyExists);
            }
            
            child.Name = payload.NewName;
        }

        dbContext.SaveChanges();
        return Ok();
    }

    
    /// <summary>
    /// Removes a child with the specified name from the logged-in parent 
    /// </summary>
    [HttpDelete]
    [Authorize]
    public IActionResult Remove(string childName)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        IQueryable<Child> childMatch = GetChildByName(parentUsername, childName);

        if (!childMatch.Any())
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }
        
        dbContext.Remove(childMatch.First());
        dbContext.SaveChanges();

        return Ok();
    }

    /// <summary>
    /// Gets a list of all children the logged-in parent is responsible for
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult All()
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        IQueryable<Child> childrenUnderParent = dbContext.Children.Include(c => c.Parent)
            .Where(c => c.Parent != null && c.Parent.Username == parentUsername);

        List<ChildResponseDTO> children = [];
        
        foreach (var c in childrenUnderParent)
        {
            children.Add(ChildResponseDTO.From(c));
        }
        
        return Ok(children);
    }

    
    private IQueryable<Child> GetChildByName(string parentUsername, string childName)
    {
        IQueryable<Child> childMatch = dbContext.Children.Include(c => c.Parent)
            .Where(c => c.Parent != null && c.Parent.Username == parentUsername && c.Name == childName);

        return childMatch;
    }
}