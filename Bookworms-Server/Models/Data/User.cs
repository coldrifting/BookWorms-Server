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