using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Utils;

namespace BookwormsServer.Models.Entities;

public class Child(string name, string parentUsername, DateOnly? dateOfBirth = null, string? readingLevel = null)
{
    [Key]
    [StringLength(22)]
    [Column(TypeName="char")]
    public string ChildId { get; set; } = Base62.FromGuid(Guid.NewGuid());
    
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    public string Name { get; set; } = name;

    [Range(0, int.MaxValue, ErrorMessage = "{0} must be a positive integer.")]
    public int ChildIcon { get; set; }

    [Range(typeof(DateOnly), "01/01/1900", "01/01/2100",
        ErrorMessage = "Child date of birth must fall between {1} and {2}.")]
    public DateOnly? DateOfBirth { get; set; } = dateOfBirth;
    
    [StringLength(6)]
    public string? ReadingLevel { get; set; } = readingLevel;

    [StringLength(64)]
    public string ParentUsername { get; set; } = parentUsername;
        
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Child classroom code must be exactly {1} characters long.")]
    public string? ClassroomCode { get; set; }

    // Navigation
    
    // Inverse property needed for extra selected child property in parent to work
    [InverseProperty(nameof(Parent.Children))]
    [ForeignKey(nameof(ParentUsername))]
    public Parent? Parent { get; set; }
    
    [ForeignKey(nameof(ClassroomCode))]
    public Classroom? Classroom { get; set; }
    
    public CompletedBookshelf? Completed { get; set; }
    public InProgressBookshelf? InProgress { get; set; }
    public ICollection<ChildBookshelf> Bookshelves { get; set; } = null!;
}