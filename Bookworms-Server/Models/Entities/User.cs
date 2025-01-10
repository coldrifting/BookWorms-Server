using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;

namespace BookwormsServer.Models.Entities;

/*
 * By default, EF takes  "Table-Per-Hierarchy" approach to inheritance.
 * That means that one table will be created for BookShelf and all its subclasses,
 * with an automatically created Discriminator column to distinguish between entities
 * of different types.
 */

[Table("Users")]
public class User(string username, byte[] hash, byte[] salt, string firstName, string lastName, UserIcon userIcon)
{
    [Key, StringLength(64, MinimumLength = 5, ErrorMessage = "User username must be between {2} and {1} characters long.")]
    public string Username { get; set; } = username;

    public byte[] Hash { get; set; } = hash;
    
    public byte[] Salt { get; set; } = salt;
    
    [StringLength(256, MinimumLength = 2, ErrorMessage = "User first name must be between {2} and {1} characters long.")]
    public string FirstName { get; set; } = firstName;
    
    [StringLength(256, MinimumLength = 2, ErrorMessage = "User last name must be between {2} and {1} characters long.")]
    public string LastName { get; set; } = lastName;

    public string[] Roles { get; set; } = [];

    [Column(TypeName = "nvarchar(64)")]
    public UserIcon UserIcon { get; set; } = userIcon;

    // Navigation

    public ICollection<Review> Reviews { get; set; } = null!;
}

public class Parent(string username, byte[] hash, byte[] salt, string firstName, string lastName, UserIcon userIcon)
    : User(username, hash, salt, firstName, lastName, userIcon)
{
    // Navigation
    
    public ICollection<Child> Children { get; set; } = null!;
}

public class Teacher(string username, byte[] hash, byte[] salt, string firstName, string lastName, UserIcon userIcon)
    : User(username, hash, salt, firstName, lastName, userIcon)
{
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Teacher classroom code must be exactly {0} characters long.")]
    public string? ClassroomCode { get; set; }
    
    // Navigation
    
    [ForeignKey(nameof(ClassroomCode))]
    public Classroom? Classroom { get; set; }
}