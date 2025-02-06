using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookwormsServer.Models.Entities;

[Table("CompletedBookshelves")]
public class CompletedBookshelf(string childId)
{
    [Key]
    public int BookshelfId { get; set; }

    [StringLength(256, ErrorMessage = "Bookshelf name cannot be longer than {0} characters.")] 
    public string Name { get; set; } = "Completed";
    
    [StringLength(22)]
    public string ChildId { get; set; } = childId;

    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child? Child { get; set; }
    
    public ICollection<CompletedBookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = null!; // Skip-navigation (many-to-many)
}

[Table("InProgressBookshelves")]
public class InProgressBookshelf(string childId)
{
    [Key]
    public int BookshelfId { get; set; }

    [StringLength(256, ErrorMessage = "Bookshelf name cannot be longer than {0} characters.")] 
    public string Name { get; set; } = "In Progress";
    
    [StringLength(22)]
    public string ChildId { get; set; } = childId;

    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child? Child { get; set; }
    
    public ICollection<InProgressBookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = null!; // Skip-navigation (many-to-many)
}

[Table("ChildBookshelves")]
public class ChildBookshelf(string name, string childId)
{
    [Key]
    public int BookshelfId { get; set; }

    [StringLength(256, ErrorMessage = "Bookshelf name cannot be longer than {0} characters.")] 
    public string Name { get; set; } = name;
    
    [StringLength(22)]
    public string ChildId { get; set; } = childId;

    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child? Child { get; set; }
    
    public ICollection<ChildBookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = null!; // Skip-navigation (many-to-many)
}

[Table("ClassroomBookshelves")]
public class ClassroomBookshelf(string name, string classroomCode)
{
    [Key]
    public int BookshelfId { get; set; }

    [StringLength(256, ErrorMessage = "Bookshelf name cannot be longer than {0} characters.")] 
    public string Name { get; set; } = name;
    
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Bookshelf classroom code must be exactly {0} characters long.")]
    public string ClassroomCode { get; set; } = classroomCode;

    // Navigation
    
    [ForeignKey(nameof(ClassroomCode))] 
    public Classroom? Classroom { get; set; }
    
    public ICollection<ClassroomBookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = null!; // Skip-navigation (many-to-many)
}