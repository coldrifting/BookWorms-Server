using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Data;

/// <summary>
/// 
/// </summary>
/// <param name="username"></param>
/// <param name="password"></param>
/// <param name="name"></param>
/// <param name="email"></param>
public class UserRegisterDTO(string username, string password, string name, string email) 
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Password { get; set; } = password;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string Name { get; set; } = name;
        
    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Email { get; set; } = email;
}

public record UserRegisterSuccessDTO(string Username, string Name, string Email, DateTime CreatedAt);

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
/// <param name="Timeout"></param>
public record UserLoginSuccessDTO(string Token, int Timeout);
