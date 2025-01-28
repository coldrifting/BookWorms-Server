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
        List<User> users = dbContext.Set<User>().ToList();

        // Must not be null due to Authorize attribute
        string loggedInUser = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        
        if (User.FindFirst(ClaimTypes.Role)?.Value != "Admin")
        {
            return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotAdmin);
        }
        
        // Put logged-in user at top of list
        for (int i = 0; i < users.Count; i++)
        {
            if (users[0].Username != loggedInUser)
            {
                continue;
            }
            
            (users[i], users[0]) = (users[0], users[i]);
            break;
        }

        List<UserDetailsDTO> usersFormatted = [];
        usersFormatted.AddRange(users.Select(UserDetailsDTO.From));

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
        string loggedInUser = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        User user = dbContext.Users.First(u => u.Username == loggedInUser);

        return Ok(UserDetailsDTO.From(user));
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
        string loggedInUser = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        User user = dbContext.Users.First(u => u.Username == loggedInUser);

        if (payload.FirstName is not null)
        {
            user.FirstName = payload.FirstName;
        }

        if (payload.LastName is not null)
        {
            user.LastName = payload.LastName;
        }

        if (payload.Icon is not null)
        {
            user.UserIcon = payload.Icon.Value;
        }

        if (payload.Password is not null)
        {
            UserService.UpdatePassword(user, payload.Password);
        }

        dbContext.SaveChanges();
        
        return Ok(UserDetailsDTO.From(user));
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
    /// <returns>The now-registered user's login token</returns>
    /// <response code="200">Success</response>
    /// <response code="422">The specified username is already taken</response>
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
            0, payload.IsParent);
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
}