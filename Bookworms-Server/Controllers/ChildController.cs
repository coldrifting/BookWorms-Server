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
    /// <returns>A list of all children under the logged-in parent</returns>
    /// <response code="200">Returns a list of all children under the logged-in parent</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
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
    /// <returns>Gets the currently selected child and their info</returns>
    /// <response code="200">Info about the currently selected child</response>
    /// <response code="204">If no child is currently selected</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
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
    /// <returns>Sets the currently selected child and then gets their info</returns>
    /// <response code="200">Info about the currently selected child</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no child with the requested name is found for the logged in parent account</response>
    [HttpPost]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
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

        return Ok(ChildResponseDTO.From(child, true));
    }
    
    /// <summary>
    /// Adds a child under the logged-in parent with the specified name.
    /// If no other children exist under this parent, the new child will
    /// be selected automatically.
    /// </summary>
    /// <returns>A list of all children under the parent and their info</returns>
    /// <response code="200">Info about all children under the logged in parent</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="422">If a child with the requested name already exists under the logged in parent account</response>
    [HttpPost]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
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
        
        // Automatically select the first added child
        Parent parent = dbContext.Parents
            .Include(parent => parent.Children)
            .First(p => p.Username == parentUsername);
        if (parent.Children.Count == 1)
        {
            SelectChild(parent, child);
        }
        
        dbContext.SaveChanges();

        return All();
    }

    
    /// <summary>
    /// Edits properties of a given child under the logged-in parent
    /// </summary>
    /// <returns>The updated details of the edited child</returns>
    /// <response code="200">Info about the  child</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no child with the requested name is found for the logged in parent account</response>
    /// <response code="422">If a child already exists under the parent with the requested name, if the classroom code is invalid, or an invalid icon is specified</response>
    [HttpPost]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
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
        
        
        if (payload.ChildIcon is not null)
        {
            if (Enum.TryParse(payload.ChildIcon, out UserIcon icon))
            {
                child.ChildIcon = icon;
            }
            else
            {
                return UnprocessableEntity(ErrorDTO.InvalidIconIndex);
            }
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
        return Ok(ChildResponseDTO.From(child, IsChildSelected(parentUsername, child.Name)));
    }

    
    /// <summary>
    /// Removes a child with the specified name from the logged-in parent 
    /// </summary>
    /// <returns>A list of all children under the parent and their info</returns>
    /// <response code="200">Info about all remaining children under the logged in parent</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no child with the requested name is found for the logged in parent account</response>
    [HttpDelete]
    [Authorize]
    [Route("/children/{childName}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
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

        // If child is selected, select the next child
        Parent parent = dbContext.Parents
            .Include(p => p.Children)
            .Include(parent => parent.SelectedChild)
            .First(p => p.Username == parentUsername);
        if (parent.SelectedChildId is not null && parent.SelectedChildId == childMatch.First().ChildId)
        {
            int index = 0;
            var children = parent.Children.ToArray();
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].Name == parent.SelectedChild?.Name)
                {
                    index = i;
                }
            }

            Child newSelectedChild = index == children.Length - 1 
                ? children[Math.Clamp(children.Length - 2, 0, children.Length - 1)] 
                : children[index + 1];

            parent.SelectedChild = newSelectedChild;
        }
        
        dbContext.Remove(childMatch.First());
        dbContext.SaveChanges();

        return All();
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

    private void SelectChild(Parent parent, Child child)
    {
        parent.SelectedChild = child;
    }
}