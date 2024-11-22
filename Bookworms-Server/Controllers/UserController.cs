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
public class UserController(AllBookwormsDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserLoginSuccessDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Login(UserLoginDTO payload)
    {
        var userMatch = dbContext.Users.Where(u => u.Username == payload.Username);
        
        if (userMatch.FirstOrDefault() is not { } candidateUser || 
            !AuthService.VerifyPassword(payload.Password, candidateUser.Hash, candidateUser.Salt))
            return BadRequest(new ErrorDTO("Invalid Credentials", "Incorrect username and/or password"));
        
        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(userMatch.First());
        return Ok(new UserLoginSuccessDTO(token, AuthService.ExpireTime));
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserRegisterSuccessDTO))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorDTO))]
    public IActionResult Register(UserRegisterDTO payload)
    {
        var userMatch = dbContext.Users.Where(l => l.Username == payload.Username);
        if (userMatch.Any())
        {
            return Conflict(new ErrorDTO("Invalid Credentials", "The specified Username already exists"));
        }

        User user = UserService.CreateUser(payload.Username, payload.Password, payload.Name, payload.Email);
        
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        return Created("/account/info", UserRegisterSuccessDTO.From(user, DateTime.Now));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<User>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    public IActionResult All()
    {
        var users = dbContext.Set<User>().ToList();

        return Ok(users);
    }
    
    [HttpGet]
    [Authorize]
    public IActionResult GetUsername()
    {
        // Example of inferring username from bearer token for authenticated routes
        string? username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        User? user = dbContext.Users.FirstOrDefault(u => u.Username == username);
        if (user is not null)
        {
            return Ok(UserInfoDTO.From(user));
        }
        
        return BadRequest(new ErrorDTO("Bad Request", "Unable to find user details"));
    }
    
}