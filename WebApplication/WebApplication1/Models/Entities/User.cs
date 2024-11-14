using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Entities;

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
    
    // Navigation

    public virtual Parent? Parent { get; set; }
    public virtual Teacher? Teacher { get; set; }

    public virtual ICollection<Review>? Reviews { get; set; }
}