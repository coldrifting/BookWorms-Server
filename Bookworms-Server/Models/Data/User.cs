using System.ComponentModel.DataAnnotations;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

/// <summary>
/// 
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
/// <param name="firstName"></param>
/// <param name="lastName"></param>
/// <param name="isParent"></param>
public class UserRegisterDTO(string username, string password, string firstName, string lastName, bool isParent)
{
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = username;

    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Password { get; set; } = password;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string FirstName { get; set; } = firstName;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string LastName { get; set; } = lastName;

    public bool IsParent { get; set; }  = isParent;
}

/// <summary>
/// 
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
public class UserLoginDTO(string username, string password)
{
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Password { get; set; } = password;
}

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
public record UserLoginSuccessDTO(string Token);

public record UserDTO(string Username, string FirstName, string LastName, string Roles, string Icon)
{
    public static UserDTO From(User userLogin)
    {
        return new(
            userLogin.Username, 
            userLogin.FirstName, 
            userLogin.LastName, 
            $"[{string.Join(", ", userLogin.Roles)}]", 
            userLogin.UserIcon.ToString());
    }
}