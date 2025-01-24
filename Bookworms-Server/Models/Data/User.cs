using System.ComponentModel.DataAnnotations;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

/// <summary>
/// 
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
/// <param name="FirstName"></param>
/// <param name="LastName"></param>
/// <param name="IsParent"></param>
public record UserRegisterDTO(
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Username,

    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Password,

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    string FirstName,

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    string LastName,

    bool IsParent);

/// <summary>
/// 
/// </summary>
/// <param name="Username"></param>
/// <param name="Password"></param>
public record UserLoginDTO(
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Username,
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string Password
    );

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
public record UserLoginSuccessDTO(string Token);

public record UserDetailsDTO(string Username, string FirstName, string LastName, string Role, string Icon)
{
    public static UserDetailsDTO From(User userLogin)
    {
        Role role;
        if (userLogin.Roles.Length > 0 && userLogin.Roles[0] == "Admin")
        {
            role = Data.Role.Admin;
        }
        else if (userLogin is Parent)
        {
            role = Data.Role.Parent;
        }
        else
        {
            role = Data.Role.Teacher;
        }
        return new(
            userLogin.Username, 
            userLogin.FirstName, 
            userLogin.LastName, 
            role.ToString(),
            userLogin.UserIcon.ToString());
    }
}

public record UserDetailsEditDTO(
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)] 
    string? FirstName = null, 
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)] 
    string? LastName = null, 
    
    string? Icon = null, 
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    string? Password = null);

public enum Role
{
    Parent,
    Teacher,
    Admin
}