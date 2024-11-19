using System.Diagnostics;
using AllOverIt.EntityFrameworkCore.Diagrams;
using AllOverIt.EntityFrameworkCore.Diagrams.D2;
using AllOverIt.EntityFrameworkCore.Diagrams.D2.Extensions;
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
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorDTO))]
    public ActionResult<UserRegisterSuccessDTO> Register(UserRegisterDTO payload)
    {
        IQueryable<User> userMatch = dbContext.Users.Where(l => l.Username == payload.Username);
        if (userMatch.Any())
        {
            return Conflict(new ErrorDTO("Invalid Credentials", "The specified Username already exists"));
        }

        byte[] hash = AuthService.HashPassword(payload.Password, out byte[] salt);
        User x = new User(payload.Username, hash, salt, payload.Name, payload.Email);

        if (x.Username == "admin")
        {
            x.Roles = ["admin"];
        }
        
        dbContext.Users.Add(x);
        dbContext.SaveChanges();

        UserRegisterSuccessDTO dto = new(x.Username, x.Name, x.Email, DateTime.Now);
        return Created("/account/info", dto);
    }

    [HttpGet, Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    public ActionResult<List<User>> Users()
    {
        List<User> users = dbContext.Set<User>().ToList();

        return users;
    }

    
	private static void AddEntityGroups(ErdOptions options)
	{
        //options.Group("web", "Web", new ShapeStyle(), entities =>
        //{
        //    entities
        //        .Add<Book>()
        //        .Add<Bookshelf>();
        //});
	}
	
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult DiagramDB()
    {
        
        
	    var erdFormatter = ErdGenerator
		    .Create<D2ErdGenerator>(options =>
		    {
			    options.Direction = ErdOptions.DiagramDirection.Right;

			    AddEntityGroups(options);
		    });
        
        
        // This generates the diagram as text
        var erd = erdFormatter.Generate(dbContext);
        Console.WriteLine(erd);

        // This generates the diagram, saves it as a text file and exports to SVG, PNG, PDF
        var exportOptions = new D2ErdExportOptions
        {      
            DiagramFileName = "..\\..\\..\\Output Examples\\sample_erd.d2",
            LayoutEngine = "elk",
            Theme = Theme.Neutral,
            Formats = [ExportFormat.Svg, ExportFormat.Png, ExportFormat.Pdf],
            StandardOutputHandler = LogOutput,
            ErrorOutputHandler = LogOutput          // Note: d2.exe seems to log everything to the error output
        };

        erdFormatter.ExportAsync(dbContext, exportOptions);

        return Ok();
    }
    
    private static void LogOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data is not null)
        {
            Console.WriteLine(e.Data);
        }
    }
    
}