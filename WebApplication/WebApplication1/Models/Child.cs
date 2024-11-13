using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Child
{
    [Key]
    public Guid Id { get; set; }
    
    [StringLength(256, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 2)]
    public string Name { get; init; } = null!;
    
    public DateOnly DateOfBirth { get; init; }
    
    [StringLength(6)]
    public string ReadingLevel { get; init; } = null!;

    [StringLength(64)]
    public string ParentUsername { get; init; } = null!;
    
    [ForeignKey(nameof(ParentUsername))]
    public Parent Parent { get; init; } = null!;

    [StringLength(6)]
    public string? ClassroomCode { get; init; }
    
    [ForeignKey(nameof(ClassroomCode))]
    public Classroom? Classroom { get; init; }
    
    
    public virtual ICollection<Completed>? Completed { get; init; }
    public virtual ICollection<Reading>? Reading { get; init; }

    // bookshelves
    // goals
}