using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Accounts - Children")]
[Route("/children/[action]")]
public class ChildController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Gets a list of all children the logged-in parent has,
    /// with their details, and shows which child is selected, if any
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult All()
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }
        
        IQueryable<Child> childrenUnderParent = dbContext.Children.Include(c => c.Parent)
            .Where(c => c.Parent != null && c.Parent.Username == parentUsername);
        
        Parent parent = dbContext.Parents
            .Include(p => p.SelectedChild)
            .First(p => p.Username == parentUsername);

        string? selectedChildName = parent.SelectedChild?.Name;
        
        List<ChildResponseDTO> children = [];
        
        foreach (Child c in childrenUnderParent)
        {
            children.Add(ChildResponseDTO.From(c, c.Name == selectedChildName ? true : null));
        }
        
        return Ok(children);
    }
    
    /// <summary>
    /// Gets the currently selected child that actions will be performed for
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Selected()
    {
        // This should never be null thanks to the authorize attribute above
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
            return NoContent();
        }

        ChildResponseDTO childResponse = ChildResponseDTO.From(parent.SelectedChild, true);

        return Ok(childResponse);
    }
    
    /// <summary>
    /// Sets the current child to perform actions for
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Select(string childName)
    {
        // This should never be null thanks to the authorize attribute above
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

        Child child = childMatch.First();

        Parent parent = dbContext.Parents.First(p => p.Username == parentUsername);

        parent.SelectedChildId = child.ChildId;

        dbContext.SaveChanges();
        
        return NoContent();
    }
    
    /// <summary>
    /// Adds a child under the logged-in parent with the specified name
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
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
            return UnprocessableEntity(ErrorDTO.ChildAlreadyExists);
        }

        Child child = new(childName, parentUsername);
        dbContext.Children.Add(child);
        dbContext.SaveChanges();
        
        return NoContent();
    }

    
    /// <summary>
    /// Edits properties of a given child under the logged-in parent
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
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

        Child child = childMatch.First();
        if (payload.DateOfBirth is not null)
        {
            child.DateOfBirth = payload.DateOfBirth;
        }

        if (payload.ReadingLevel is not null)
        {
            child.ReadingLevel = payload.ReadingLevel;
        }

        if (payload.ClassroomCode is not null)
        {
            IQueryable<Classroom> classrooms = dbContext.Classrooms.Where(c => c.ClassroomCode == payload.ClassroomCode);
            if (classrooms.Any())
            {
                child.ClassroomCode = payload.ClassroomCode;
            }
            else
            {
                return UnprocessableEntity(ErrorDTO.ClassroomNotFound);
            }
        }

        if (payload.NewName is not null)
        {
            IQueryable<Child> childMatchNew = GetChildByName(parentUsername, payload.NewName);
            if (childMatchNew.Any())
            {
                return UnprocessableEntity(ErrorDTO.ChildAlreadyExists);
            }
            
            child.Name = payload.NewName;
        }

        dbContext.SaveChanges();
        return Ok(ChildResponseDTO.From(child, IsChildSelected(parentUsername, childName)));
    }

    
    /// <summary>
    /// Removes a child with the specified name from the logged-in parent 
    /// </summary>
    [HttpDelete]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
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

        return NoContent();
    }

    
    private IQueryable<Child> GetChildByName(string parentUsername, string childName)
    {
        IQueryable<Child> childMatch = dbContext.Children.Include(c => c.Parent)
            .Where(c => c.Parent != null && c.Parent.Username == parentUsername && c.Name == childName);

        return childMatch;
    }

    private bool IsChildSelected(string parentUsername, string childName)
    {
        return dbContext.Parents
            .Include(p => p.SelectedChild)
            .Any(p => p.Username == parentUsername && 
                            p.SelectedChild != null && 
                            p.SelectedChild.Name == childName);
    }
}