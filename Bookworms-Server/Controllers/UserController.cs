using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Accounts")]
[Route("user/[action]")]
public class UserController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Logs in a user using the given login payload
    /// </summary>
    /// <param name="payload">The user login data payload</param>
    /// <returns>The now-logged-in user's JWT token and session timeout</returns>
    /// <response code="200">Returns the session data</response>
    /// <response code="400">If the provided credentials are invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginSuccessDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Login(UserLoginDTO payload)
    {
        IQueryable<User> userMatch = dbContext.Users.Where(u => u.Username == payload.Username);
        
        if (userMatch.FirstOrDefault() is not { } candidateUser || 
            !AuthService.VerifyPassword(payload.Password, candidateUser.Hash, candidateUser.Salt))
            return BadRequest(ErrorDTO.LoginFailure);
        
        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(userMatch.First());
        return Ok(new UserLoginSuccessDTO(token));
    }

    /// <summary>
    /// Registers a new user 
    /// </summary>
    /// <param name="payload">The data with which to register the new user</param>
    /// <returns>The now-registered user's data</returns>
    /// <response code="200">Creates and returns session data for the new user</response>
    /// <response code="422">If the specified username is already taken</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginSuccessDTO))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorDTO))]
    public IActionResult Register(UserRegisterDTO payload)
    {
        IQueryable<User> userMatch = dbContext.Users.Where(l => l.Username == payload.Username);
        if (userMatch.Any())
        {
            return UnprocessableEntity(ErrorDTO.UsernameAlreadyExists);
        }

        User user = UserService.CreateUser(payload.Username, payload.Password, payload.FirstName, payload.LastName,
            UserIcon.Icon1, payload.IsParent);
        switch (user)
        {
            case Parent p:
                dbContext.Parents.Add(p);
                break;
            case Teacher t:
                dbContext.Teachers.Add(t);
                break;
            default:
                dbContext.Users.Add(user);
                break;
        }

        dbContext.SaveChanges();

        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(userMatch.First());
        return Ok(new UserLoginSuccessDTO(token));
    }

    /// <summary>
    /// Returns all the registered users
    /// </summary>
    /// <returns>A list of all Users</returns>
    /// <response code="200">Returns a list of all users</response>
    /// <response code="401">If the user is not logged in</response>
    /// <response code="403">If the user is not an admin</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<UserDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    public IActionResult All()
    {
        var users = dbContext.Set<User>().ToList();

        // Must not be null due to Authorize attribute
        string loggedInUser = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        if (User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotAdmin);
        }
        
        // Put logged-in user at top of list
        for (int i = 0; i < users.Count; i++)
        {
            if (users[0].Username == loggedInUser)
            {
                (users[i], users[0]) = (users[0], users[i]);
                break;
            }
        }

        List<UserDTO> usersFormatted = [];
        usersFormatted.AddRange(users.Select(UserDTO.From));

        return Ok(usersFormatted);
    }
}