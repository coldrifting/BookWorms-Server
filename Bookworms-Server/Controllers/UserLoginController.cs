using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[AllowAnonymous]
[Tags("Users")]
[Route("user/[action]")]
public class UserLoginController(BookwormsDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Logs in a user using the given login payload
    /// </summary>
    /// <param name="payload">The user login data payload</param>
    /// <returns>The now-logged-in user's JWT token and session timeout</returns>
    /// <response code="200">Success</response>
    /// <response code="400">The provided credentials are invalid</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginSuccessResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
    public IActionResult Login(UserLoginRequest payload)
    {
        User? user = dbContext.Users.Find(payload.Username);

        if (user is null || !AuthService.VerifyPassword(payload.Password, user.Hash, user.Salt))
        {
            return BadRequest(ErrorResponse.LoginFailure);
        }

        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(user);
        return Ok(new UserLoginSuccessResponse(token));
    }

    /// <summary>
    /// Registers a new user 
    /// </summary>
    /// <param name="payload">The data with which to register the new user</param>
    /// <returns>The now-registered user's login token</returns>
    /// <response code="200">Success</response>
    /// <response code="422">The specified username is already taken</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginSuccessResponse))]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(ErrorResponse))]
    public IActionResult Register(UserRegisterRequest payload)
    {
        if (dbContext.Users.Find(payload.Username) is not null)
        {
            return UnprocessableEntity(ErrorResponse.UsernameAlreadyExists);
        }

        User newUser = UserService.CreateUser(
            payload.Username, 
            payload.Password, 
            payload.FirstName, 
            payload.LastName,
            0, 
            payload.IsParent);
        
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
        return Ok(new UserLoginSuccessResponse(token));
    }
}