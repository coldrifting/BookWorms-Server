using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Users")]
[Route("user/[action]")]
public class UserController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Returns all the registered users
    /// </summary>
    /// <returns>A list of all Users</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not an admin</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserDetailsDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    public IActionResult All()
    {
        string loggedInUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var loggedInUser = dbContext.Users.Find(loggedInUsername)!;

        if (!loggedInUser.IsAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotAdmin);
        }
        
        List<UserDetailsDTO> usersFormatted = dbContext.Users.Select(UserDetailsDTO.From).ToList();

        return Ok(usersFormatted);
    }

    /// <summary>
    /// Returns information about the currently logged in user
    /// </summary>
    /// <returns>Details about the current user</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    [HttpGet]
    [Authorize]
    [Route("/user/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    public IActionResult Details()
    {
        // Must not be null due to Authorize attribute
        string loggedInUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        User loggedInUser = dbContext.Users.Find(loggedInUsername)!;

        return Ok(UserDetailsDTO.From(loggedInUser));
    }

    /// <summary>
    /// Edits properties of the currently logged in user
    /// </summary>
    /// <returns>Details about the current user</returns>
    /// <response code="200">Success</response>
    /// <response code="401">The user is not logged in</response>
    [HttpPut]
    [Authorize]
    [Route("/user/details")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsDTO))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    public IActionResult Edit(UserDetailsEditDTO payload)
    {
        // Must not be null due to Authorize attribute
        string loggedInUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        User loggedInUser = dbContext.Users.Find(loggedInUsername)!;

        if (payload.FirstName is not null)
        {
            loggedInUser.FirstName = payload.FirstName;
        }

        if (payload.LastName is not null)
        {
            loggedInUser.LastName = payload.LastName;
        }

        if (payload.Icon is not null)
        {
            loggedInUser.UserIcon = payload.Icon.Value;
        }

        if (payload.Password is not null)
        {
            UserService.UpdatePassword(loggedInUser, payload.Password);
        }

        dbContext.SaveChanges();
        
        return Ok(UserDetailsDTO.From(loggedInUser));
    }
    
    /// <summary>
    /// Logs in a user using the given login payload
    /// </summary>
    /// <param name="payload">The user login data payload</param>
    /// <returns>The now-logged-in user's JWT token and session timeout</returns>
    /// <response code="200">Success</response>
    /// <response code="400">The provided credentials are invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginSuccessDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Login(UserLoginDTO payload)
    {
        User? user = dbContext.Users.Find(payload.Username);

        if (user is null || !AuthService.VerifyPassword(payload.Password, user.Hash, user.Salt))
        {
            return BadRequest(ErrorDTO.LoginFailure);
        }

        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(user);
        return Ok(new UserLoginSuccessDTO(token));
    }

    /// <summary>
    /// Registers a new user 
    /// </summary>
    /// <param name="payload">The data with which to register the new user</param>
    /// <returns>The now-registered user's login token</returns>
    /// <response code="200">Success</response>
    /// <response code="422">The specified username is already taken</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginSuccessDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Register(UserRegisterDTO payload)
    {
        User? existingUser = dbContext.Users.Find(payload.Username);
        if (existingUser is not null)
        {
            return UnprocessableEntity(ErrorDTO.UsernameAlreadyExists);
        }

        User newUser = UserService.CreateUser(payload.Username, payload.Password, payload.FirstName, payload.LastName,
            0, payload.IsParent);
        switch (newUser)
        {
            case Teacher t:
                dbContext.Teachers.Add(t);
                break;
            case Parent p:
                dbContext.Parents.Add(p);
                break;
        }

        dbContext.SaveChanges();

        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(newUser);
        return Ok(new UserLoginSuccessDTO(token));
    }

    /// <summary>
    /// Deletes the logged in or specified user 
    /// </summary>
    /// <param name="username">The user to delete if not deleting own account</param>
    /// <response code="204">Success</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The user to delete does not exist</response>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public IActionResult Delete([FromQuery] string? username = null)
    {
        string loggedInUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var loggedInUser = dbContext.Users.Find(loggedInUsername)!;
            
        // Own account deletion
        if (username is null)
        {
            dbContext.Users.Remove(loggedInUser);
            dbContext.SaveChanges();
            return NoContent();
        }
        
        // Delete other accounts
        if (!loggedInUser.IsAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotAdmin);
        }
        
        var userToDelete = dbContext.Users.Find(username);
        if (userToDelete is null)
        {
            return UnprocessableEntity(ErrorDTO.UserNotFound);
        }
        
        dbContext.Users.Remove(userToDelete);
        dbContext.SaveChanges();
        return NoContent();
    }
}