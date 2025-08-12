using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[Tags("Users - Management")]
[Route("user/[action]")]
public class UserDetailsController(BookwormsDbContext context) : AuthControllerBase(context)
{
    /// <summary>
    /// Returns all the registered users
    /// </summary>
    /// <returns>A list of all Users</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not an admin</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserDetailsResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    public IActionResult All()
    {
        return CurrentUser is Admin
            ? Ok(DbContext.Users.Select(user => user.ToResponse()).ToList())
            : Forbidden(ErrorResponse.UserNotAdmin);
    }

    /// <summary>
    /// Returns information about the currently logged in user
    /// </summary>
    /// <returns>Details about the current user</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    [HttpGet]
    [Route("/user/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    public IActionResult Details()
    {
        return Ok(CurrentUser.ToResponse());
    }

    /// <summary>
    /// Edits properties of the currently logged in user
    /// </summary>
    /// <returns>Details about the current user</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    [HttpPut]
    [Route("/user/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    public IActionResult Edit(UserDetailsEditRequest payload)
    {
        // EF will only update DB state if a value has changed different
        CurrentUser.FirstName = payload.FirstName ?? CurrentUser.FirstName;
        CurrentUser.LastName = payload.LastName ?? CurrentUser.LastName;
        CurrentUser.UserIcon = payload.Icon ?? CurrentUser.UserIcon;

        if (payload.Password is not null)
        {
            UserService.UpdatePassword(CurrentUser, payload.Password);
        }

        DbContext.SaveChanges();
        
        return Ok(CurrentUser.ToResponse());
    }

    /// <summary>
    /// Deletes the logged in or specified user 
    /// </summary>
    /// <param name="username">The user to delete if not deleting own account</param>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not an admin</response>
    /// <response code="404">The user to delete does not exist</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public IActionResult Delete([FromQuery] string? username = null)
    {
        // self account deletion
        if (username is null)
        {
            DbContext.Users.Remove(CurrentUser);
            DbContext.SaveChanges();
            return NoContent();
        }
        
        // Delete other accounts
        if (CurrentUser is not Admin)
        {
            return Forbidden(ErrorResponse.UserNotAdmin);
        }
        
        User? userToDelete = DbContext.Users.Find(username);
        if (userToDelete is null)
        {
            return UnprocessableEntity(ErrorResponse.UserNotFound);
        }
        
        DbContext.Users.Remove(userToDelete);
        DbContext.SaveChanges();
        return NoContent();
    }
}