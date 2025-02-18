using System.Security.Claims;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

/// <summary>
/// Contains helper properties and methods for controllers that contain only authorized routes.
/// <br/>
/// Due to a fun quirk of primary constructors, only use the <c>DbContext</c> property defined in this class.
/// Don't use the context object directly in derived controllers.
/// </summary>
[Authorize]
[ApiController]
public class AuthControllerBase : ControllerBase
{
    protected AuthControllerBase(BookwormsDbContext context)
    {
        DbContext = context;
    }

    protected readonly BookwormsDbContext DbContext;
    
    private User? _user;
    protected User CurrentUser
    {
        get
        {
            if (_user == null)
            {
		        string loggedInUsername = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
		        _user = DbContext.Users.Find(loggedInUsername)!;
            }
            return _user;
        }
    }

    protected Child? CurrentUserChild(string childId)
    {
        return DbContext.Children.Find(childId) is { } child && child.ParentUsername == CurrentUser.Username
            ? child
            : null;
    }
    
    protected List<Child> CurrentUserChildren()
    {
        return DbContext.Children.Where(child => child.ParentUsername == CurrentUser.Username).ToList();
    }
}