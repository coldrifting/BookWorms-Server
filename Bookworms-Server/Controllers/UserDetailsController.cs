using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Authorize]
[Tags("Users")]
[Route("user/[action]")]
public class UserDetailsController(BookwormsDbContext dbContext) : ControllerBase
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
        return dbContext.Users.CurrentUser(User) is Admin
            ? Ok(dbContext.Users.Select(user => user.ToResponse()).ToList())
            : StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotAdmin);
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
        return Ok(dbContext.Users.CurrentUser(User).ToResponse());
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
        User user = dbContext.Users.CurrentUser(User);

        // EF will only update DB state if a value has changed different
        user.FirstName = payload.FirstName ?? user.FirstName;
        user.LastName = payload.LastName ?? user.LastName;
        user.UserIcon = payload.Icon ?? user.UserIcon;

        if (payload.Password is not null)
        {
            UserService.UpdatePassword(user, payload.Password);
        }

        dbContext.SaveChanges();
        
        return Ok(user.ToResponse());
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
        User user = dbContext.Users.CurrentUser(User);
            
        // self account deletion
        if (username is null)
        {
            dbContext.Users.Remove(user);
            dbContext.SaveChanges();
            return NoContent();
        }
        
        // Delete other accounts
        if (user is not Admin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotAdmin);
        }
        
        User? userToDelete = dbContext.Users.Find(username);
        if (userToDelete is null)
        {
            return UnprocessableEntity(ErrorResponse.UserNotFound);
        }
        
        dbContext.Users.Remove(userToDelete);
        dbContext.SaveChanges();
        return NoContent();
    }
}