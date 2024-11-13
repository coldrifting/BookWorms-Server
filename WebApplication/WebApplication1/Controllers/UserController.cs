using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers;

[ApiController]
[Tags("Accounts")]
[Route("account/[action]")]
public class UserController(BookwormsDbContext dbContext) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public ActionResult<UserLoginSuccessDTO> Login(UserLoginDTO payload)
    {
        IQueryable<User> userMatch = dbContext.Users.Where(u => u.Username == payload.Username);

        if (userMatch.FirstOrDefault() is not { } candidateUser)
            return BadRequest(new ErrorDTO("Invalid Credentials", "Incorrect username and/or password"));
        
        if (!AuthService.VerifyPassword(payload.Password, candidateUser.Hash, candidateUser.Salt))
            return BadRequest(new ErrorDTO("Invalid Credentials", "Incorrect username and/or password"));
        
        // Send JWT token to avoid expensive hash calls for each authenticated endpoint
        string token = AuthService.GenerateToken(userMatch.First());
        return new UserLoginSuccessDTO(token, AuthService.ExpireTime);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorDTO))]
    public ActionResult<UserLoginSuccessDTO> Register(UserRegisterDTO payload)
    {
        IQueryable<User> userMatch = dbContext.Users.Where(l => l.Username == payload.Username);
        if (userMatch.Any())
        {
            return BadRequest(new ErrorDTO("Invalid Credentials", "The specified Username already exists"));
        }

        byte[] hash = AuthService.HashPassword(payload.Password, out byte[] salt);
        User x = new User(payload.Username, hash, salt, payload.Name, payload.Email, [""]);

        dbContext.Users.Add(x);
        dbContext.SaveChanges();

        return Ok();
    }
}