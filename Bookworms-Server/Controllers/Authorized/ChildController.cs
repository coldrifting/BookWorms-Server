using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[Tags("Children")]
[Route("/children/[action]")]
public class ChildDetailsController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Gets a list of all children the logged-in parent has with their details
    /// </summary>
    /// <returns>A list of all children under the logged-in parent</returns>
    /// <response code="200">Returns a list of all children under the logged-in parent</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    public IActionResult All()
    {
        return CurrentUser is Parent
            ? Ok(CurrentUserChildren().Select(c => c.ToResponse()).ToList())
            : Forbidden(ErrorResponse.UserNotParent);
    }


    /// <summary>
    /// Adds a child under the logged-in parent with the specified name.
    /// </summary>
    /// <remarks>
    /// If no other children exist under this parent, the new child will
    /// be selected automatically.
    /// </remarks>
    /// <returns>A list of all children under the parent and their info</returns>
    /// <response code="201">Success. Child ID Included in Location header</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(List<ChildResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    public IActionResult Add(string childName)
    {
        if (CurrentUser is not Parent parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        Child child = new(childName, parent.Username);
        child.Completed = new(child.ChildId);
        child.InProgress = new(child.ChildId);
        DbContext.Children.Add(child);

        DbContext.SaveChanges();

        return Created(
            $"/children/{child.ChildId}", 
            CurrentUserChildren().Select(c => c.ToResponse()).ToList());
    }
}


[Tags("Children")]
[Route("/children/{childId}/[action]")]
public class ChildEditController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Edits properties of a given child under the logged-in parent
    /// </summary>
    /// <returns>The updated details of the edited child</returns>
    /// <response code="200">Info about the child</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child's ID is invalid, or is not managed by the logged in parent</response>
    /// <response code="422">The classroom code is invalid, or an invalid icon is specified</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult Edit([FromRoute] string childId, [FromBody] ChildEditRequest payload)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
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

        DbContext.SaveChanges();
        return Ok(child.ToResponse());
    }

    
    /// <summary>
    /// Deletes the specified child that is under the logged-in parent 
    /// </summary>
    /// <returns>A list of all remaining children under the parent</returns>
    /// <response code="200">Info about all remaining children under the logged in parent</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child's ID is invalid, or is not managed by the logged in parent</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChildResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Remove(string childId)
    {
        if (CurrentUser is not Parent)
        {
            return Forbidden(ErrorResponse.UserNotParent);
        }

        if (CurrentUserChild(childId) is not { } child)
        {
            return NotFound(ErrorResponse.ChildNotFound);
        }
        
        DbContext.Remove(child);
        DbContext.SaveChanges();
        
        return Ok(CurrentUserChildren().Select(c => c.ToResponse()).ToList());
    }
}