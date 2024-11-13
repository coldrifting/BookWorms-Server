using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

/// <summary>
/// Main user object, stored in DB via Entity Framework 
/// </summary>
/// <param name="username"></param>
/// <param name="hash"></param>
/// <param name="salt"></param>
/// <param name="name"></param>
/// <param name="email"></param>
public class User(string username, byte[] hash, byte[] salt, string name, string email)
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = username;

    public byte[] Hash { get; set; } = hash;
    public byte[] Salt { get; set; } = salt;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string Name { get; set; } = name;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Email { get; set; } = email;

    public string[] Roles { get; set; } = [];
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
    public string Username { get; init; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Password { get; init; } = password;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string Name { get; init; } = name;
        
    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Email { get; init; } = email;
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
    public string Username { get; init; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Password { get; init; } = password;
}

/// <summary>
/// 
/// </summary>
/// <param name="Token"></param>
/// <param name="Timeout"></param>
public record UserLoginSuccessDTO(string Token, int Timeout);
