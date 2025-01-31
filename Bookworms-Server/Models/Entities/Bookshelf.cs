using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookwormsServer.Models.Entities;

[Table("Bookshelves")]
public abstract class Bookshelf(string name)
{
    [Key]
    public int BookshelfId { get; set; }

    [StringLength(256, ErrorMessage = "Bookshelf name cannot be longer than {0} characters.")] 
    public string Name { get; set; } = name;
    
    // Navigation
    
    public ICollection<BookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = null!; // Skip-navigation (many-to-many)
}


[Table("CompletedBookshelves")]
public class CompletedBookshelf(string childId) : Bookshelf("Completed")
{
    [Column(nameof(ChildId))]
    [StringLength(22)]
    public string ChildId { get; set; } = childId;

    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child? Child { get; set; }
}

[Table("InProgressBookshelves")]
public class InProgressBookshelf(string childId) : Bookshelf("In Progress")
{
    [Column(nameof(ChildId))]
    [StringLength(22)]
    public string ChildId { get; set; } = childId;

    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child? Child { get; set; }
}

[Table("ChildBookshelves")]
public class ChildBookshelf(string name, string childId) : Bookshelf(name)
{
    [Column(nameof(ChildId))]
    [StringLength(22)]
    public string ChildId { get; set; } = childId;

    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child? Child { get; set; }
}

[Table("ClassroomBookshelves")]
public class ClassroomBookshelf(string name, string classroomCode) : Bookshelf(name)
{
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Bookshelf classroom code must be exactly {0} characters long.")]
    public string ClassroomCode { get; set; } = classroomCode;

    // Navigation
    
    [ForeignKey(nameof(ClassroomCode))] 
    public Classroom? Classroom { get; set; }
}