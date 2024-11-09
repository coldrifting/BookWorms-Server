using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class UserLoginDTO(string username, string password)
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 6)]
    public string Username { get; init; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 6)]
    public string Password { get; init; } = password;
}

public class UserRegisterDTO(string username, string password, string name, string email) 
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 6)]
    public string Username { get; init; } = username;
    
    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 6)]
    public string Password { get; init; } = password;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string Name { get; init; } = name;
        
    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Email { get; init; } = email;
}

// Main user object, stored in DB via Entity Framework
public class User(string username, byte[] hash, byte[] salt, string name, string email, string[] roles)
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 6)]
    public string Username { get; init; } = username;
    
    public byte[] Hash { get; init; } = hash;
    public byte[] Salt { get; init; } = salt;

    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string Name { get; init; } = name;
    
    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Email { get; init; } = email;
    
    public string[] Roles { get; init; } = roles;
}

public record UserLoginSuccessDTO(string Token, int Timeout);
