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

        return dbContext.Parents.Any(p => p.Username == parentUsername) 
            ? Ok(GetAllChildren(parentUsername)) 
            : StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
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
    /// Adds a child under the logged-in parent with the specified name.
    /// If no other children exist under this parent, the new child will
    /// be selected automatically.
    /// </summary>
    /// <returns>A list of all children under the parent and their info</returns>
    /// <response code="201">A child was successfully created</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    [HttpPost]
    [Authorize]
    [Route("/children/[action]")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(List<ChildResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    public IActionResult Add(string childName)
    {
        // This should never be null thanks to the authorize attribute above
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
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

        return Created($"/children/{child.ChildId.ToString()}", GetAllChildren(parentUsername));
    }
    
    
    /// <summary>
    /// Sets the current child to perform actions for
    /// </summary>
    /// <returns>Sets the currently selected child and then gets their info</returns>
    /// <response code="200">Info about the currently selected child</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no child with the requested id is found under the logged in parent account</response>
    [HttpPut]
    [Authorize]
    [Route("/children/{childId:guid}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Select(Guid childId)
    {
        // This should never be null thanks to the authorize attribute above
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children.FirstOrDefault(c => c.ChildId == childId);
        if (child is null || child.ParentUsername != parentUsername)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        Parent parent = dbContext.Parents.First(p => p.Username == parentUsername);

        parent.SelectedChildId = child.ChildId;

        dbContext.SaveChanges();

        return Ok(ChildResponseDTO.From(child, true));
    }

    
    /// <summary>
    /// Edits properties of a given child under the logged-in parent
    /// </summary>
    /// <returns>The updated details of the edited child</returns>
    /// <response code="200">Info about the child</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If the child id is invalid, or is not managed by the logged in parent</response>
    /// <response code="422">If the classroom code is invalid, or an invalid icon is specified</response>
    [HttpPut]
    [Authorize]
    [Route("/children/{childId:guid}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Edit(Guid childId, ChildEditDTO payload)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children.FirstOrDefault(c => c.ChildId == childId);
        if (child is null || child.ParentUsername != parentUsername)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        if (payload.NewName is not null)
        {
            child.Name = payload.NewName;
        }

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
            if (!Enum.TryParse(payload.ChildIcon, out UserIcon icon))
            {
                return UnprocessableEntity(ErrorDTO.InvalidIconIndex);
            }

            child.ChildIcon = icon;
        }

        if (payload.ClassroomCode is not null)
        {
            if (!dbContext.Classrooms.Any(c => c.ClassroomCode == payload.ClassroomCode))
            {
                return UnprocessableEntity(ErrorDTO.ClassroomNotFound);
            }
            
            child.ClassroomCode = payload.ClassroomCode;
        }

        dbContext.SaveChanges();
        return Ok(ChildResponseDTO.From(child, IsChildSelected(parentUsername, child.Name)));
    }

    
    /// <summary>
    /// Deletes the specified child that is under the logged-in parent 
    /// </summary>
    /// <returns>A list of all children under the parent and their info</returns>
    /// <response code="200">Info about all remaining children under the logged in parent</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not a parent</response>
    /// <response code="404">If no child with the requested id is found under the logged in parent account</response>
    [HttpDelete]
    [Authorize]
    [Route("/children/{childId:guid}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Remove(Guid childId)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children.FirstOrDefault(c => c.ChildId == childId);
        if (child is null || child.ParentUsername != parentUsername)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        // If child is selected, select the next child
        Parent parent = dbContext.Parents
            .Include(p => p.Children)
            .Include(parent => parent.SelectedChild)
            .First(p => p.Username == parentUsername);
        if (parent.SelectedChildId is not null && parent.SelectedChildId == child.ChildId)
        {
            List<Child> children = parent.Children.ToList();
            int index = children.IndexOf(child);
            
            children.Remove(child);

            if (children.Count > 0)
            {
                int newIndex = Math.Clamp(index, 0, children.Count - 1);
                Child newSelectedChild = children[newIndex];

                parent.SelectedChild = newSelectedChild;
            }
            else
            {
                parent.SelectedChild = null;
            }
        }
        
        dbContext.Remove(child);
        dbContext.SaveChanges();

        return All();
    }

    
    private List<ChildResponseDTO> GetAllChildren(string parentUsername)
    {
        Parent parent = dbContext.Parents
            .Include(parent => parent.SelectedChild)
            .Include(parent => parent.Children)
            .First(p => p.Username == parentUsername);
        
        Guid? selectedChildId = parent.SelectedChildId;
        
        List<ChildResponseDTO> children = [];
        foreach (Child c in parent.Children)
        {
            children.Add(ChildResponseDTO.From(c, c.ChildId == selectedChildId ? true : null));
        }

        return children;
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