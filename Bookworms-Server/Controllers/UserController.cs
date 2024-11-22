using System.Security.Claims;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Tags("Accounts")]
[Route("account/[action]")]
public class UserController(AllBookwormsDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public IActionResult Login(UserLoginDTO payload)
    {
        IQueryable<User> userMatch = dbContext.Users.Where(u => u.Username == payload.Username);
        
        if (userMatch.FirstOrDefault() is not { } candidateUser)
            return BadRequest(new ErrorDTO("Invalid Credentials", "Incorrect username and/or password"));
        
        if (!AuthService.VerifyPassword(payload.Password, candidateUser.Hash, candidateUser.Salt))
            return BadRequest(new ErrorDTO("Invalid Credentials", "Incorrect username and/or password"));
        
        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(userMatch.First());
        return Ok(new UserLoginSuccessDTO(token, AuthService.ExpireTime));
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorDTO))]
    public ActionResult Register(UserRegisterDTO payload)
    {
        IQueryable<User> userMatch = dbContext.Users.Where(l => l.Username == payload.Username);
        if (userMatch.Any())
        {
            return Conflict(new ErrorDTO("Invalid Credentials", "The specified Username already exists"));
        }

        User user = UserService.CreateUser(payload.Username, payload.Password, payload.Name, payload.Email);

        if (user.Username == "admin")
        {
            user.Roles = ["admin"];
        }
        
        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        UserRegisterSuccessDTO dto = new(user.Username, user.Name, user.Email, DateTime.Now);
        return Created("/account/info", dto);
    }

    [HttpGet, Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    public IActionResult Users()
    {
        List<User> users = dbContext.Set<User>().ToList();

        return Ok(users);
    }
    
    [HttpGet]
    [Authorize]
    public IActionResult GetUsername()
    {
        // Example of infering username from bearer token for authenticated routes
        string? username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Ok(username);
    }
    
}