using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Children")]
[Route("/children/[action]")]
public class ChildController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Gets a list of all children the logged-in parent has with their details
    /// </summary>
    /// <returns>A list of all children under the logged-in parent</returns>
    /// <response code="200">Returns a list of all children under the logged-in parent</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
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
    /// Adds a child under the logged-in parent with the specified name.
    /// If no other children exist under this parent, the new child will
    /// be selected automatically.
    /// </summary>
    /// <returns>A list of all children under the parent and their info</returns>
    /// <response code="201">Success. Child ID Included in Location header</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    [HttpPost]
    [Authorize]
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
        
        dbContext.SaveChanges();

        return Created($"/children/{child.ChildId}", GetAllChildren(parentUsername));
    }

    
    /// <summary>
    /// Edits properties of a given child under the logged-in parent
    /// </summary>
    /// <returns>The updated details of the edited child</returns>
    /// <response code="200">Info about the child</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child ID is invalid, or is not managed by the logged in parent</response>
    /// <response code="422">The classroom code is invalid, or an invalid icon is specified</response>
    [HttpPut]
    [Authorize]
    [Route("/children/{childId}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponseDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Edit([FromRoute] string childId, [FromBody] ChildEditDTO payload)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children.Find(childId);
        if (child is null || child.ParentUsername != parentUsername)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }

        if (payload.ClassroomCode is not null)
        {
            if (!dbContext.Classrooms.Any(c => c.ClassroomCode == payload.ClassroomCode))
            {
                return UnprocessableEntity(ErrorDTO.ClassroomNotFound);
            }
            
            child.ClassroomCode = payload.ClassroomCode;
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
            child.ChildIcon = payload.ChildIcon.Value;
        }

        dbContext.SaveChanges();
        return Ok(ChildResponseDTO.From(child));
    }

    
    /// <summary>
    /// Deletes the specified child that is under the logged-in parent 
    /// </summary>
    /// <returns>A list of all children under the parent and their info</returns>
    /// <response code="200">Info about all remaining children under the logged in parent</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child ID is invalid, or is not managed by the logged in parent</response>
    [HttpDelete]
    [Authorize]
    [Route("/children/{childId}/[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponseDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Remove(string childId)
    {
        string parentUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        if (!dbContext.Parents.Any(p => p.Username == parentUsername))
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
        }

        Child? child = dbContext.Children.Find(childId);
        if (child is null || child.ParentUsername != parentUsername)
        {
            return NotFound(ErrorDTO.ChildNotFound);
        }
        
        dbContext.Remove(child);
        dbContext.SaveChanges();

        return All();
    }

    
    private List<ChildResponseDTO> GetAllChildren(string parentUsername)
    {
        return dbContext.Children
            .Where(c => c.ParentUsername == parentUsername)
            .AsEnumerable()
            .Select(ChildResponseDTO.From)
            .ToList();
    }
}