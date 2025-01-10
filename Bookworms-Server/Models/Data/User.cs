using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Data;

/// <summary>
/// 
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
/// <param name="firstName"></param>
/// <param name="lastName"></param>
public class UserRegisterDTO(string username, string password, string firstName, string lastName) 
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Password { get; set; } = password;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string FirstName { get; set; } = firstName;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string LastName { get; set; } = lastName;
}

/// <summary>
/// 
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
public class UserLoginDTO(string username, string password)
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Password { get; set; } = password;
}

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
public record UserLoginSuccessDTO(string Token);