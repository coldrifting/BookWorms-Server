using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookwormsServer.Models.Entities;

/*
 * By default, EF takes  "Table-Per-Hierarchy" approach to inheritance.
 * That means that one table will be created for BookShelf and all its subclasses,
 * with an automatically created Discriminator column to distinguish between entities
 * of different types.
 */

[Table("Users")]
public class User(string username, byte[] hash, byte[] salt, string name, string email)
{
    [Key, StringLength(64, MinimumLength = 5, ErrorMessage = "User username must be between {2} and {1} characters long.")]
    public string Username { get; set; } = username;

    public byte[] Hash { get; set; } = hash;
    
    public byte[] Salt { get; set; } = salt;
    
    [StringLength(256, MinimumLength = 2, ErrorMessage = "User name must be between {2} and {1} characters long.")]
    public string Name { get; set; } = name;
    
    [StringLength(256, MinimumLength = 5, ErrorMessage = "User email must be between {2} and {1} characters long.")]
    public string Email { get; set; } = email;

    public string[] Roles { get; set; } = [];
    
    // Navigation

    public ICollection<Review> Reviews { get; set; } = null!;
}

public class Parent(string username, byte[] hash, byte[] salt, string name, string email)
    : User(username, hash, salt, name, email)
{
    // Navigation
    
    public ICollection<Child> Children { get; set; } = null!;
}

public class Teacher(string username, byte[] hash, byte[] salt, string name, string email)
    : User(username, hash, salt, name, email)
{
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Teacher classroom code must be exactly {0} characters long.")]
    public string? ClassroomCode { get; set; }
    
    // Navigation
    
    [ForeignKey(nameof(ClassroomCode))]
    public Classroom? Classroom { get; set; }
}