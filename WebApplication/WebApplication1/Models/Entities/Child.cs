using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities;

public class Child(string name, DateOnly dateOfBirth, string parentUsername, string? readingLevel = null)
{
    [Key]
    public Guid ChildId { get; set; }
    
    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string Name { get; set; } = name;

    public DateOnly DateOfBirth { get; set; } = dateOfBirth;
    
    [StringLength(6)]
    public string? ReadingLevel { get; set; } = readingLevel;

    [StringLength(64)]
    public string ParentUsername { get; set; } = parentUsername;
        
    [StringLength(6)]
    public string? ClassroomCode { get; set; }
    
    // Navigation
    
    [ForeignKey(nameof(ParentUsername))]
    public Parent Parent { get; set; } = null!;
    
    [ForeignKey(nameof(ClassroomCode))]
    public virtual Classroom? Classroom { get; init; }
    
    public virtual ICollection<Completed>? Completed { get; set; }
    public virtual ICollection<Reading>? Reading { get; set; }
    public virtual ICollection<BookshelfChild>? Bookshelves { get; set; }
}