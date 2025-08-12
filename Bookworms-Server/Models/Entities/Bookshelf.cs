using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;

namespace BookwormsServer.Models.Entities;

public abstract class Bookshelf
{
    [Key]
    public int BookshelfId { get; set; }

    // Navigation
    
    public ICollection<Book> Books { get; set; } = null!; // Skip-navigation (many-to-many)

    public abstract BookshelfResponse ToResponse(int numBooks = int.MaxValue);
}

[Table("CompletedBookshelves")]
public class CompletedBookshelf(string childId) : Bookshelf
{
    [StringLength(22)]
    public string ChildId { get; set; } = childId;
    
    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child Child { get; set; } = null!; // 1:1 - Required reference navigation to principal

    public ICollection<CompletedBookshelfBook> CompletedBookshelfBooks { get; set; } = null!;
    
    public override BookshelfResponse ToResponse(int numBooks = int.MaxValue)
    {
        List<CompletedBookshelfBook> completion = CompletedBookshelfBooks.Take(numBooks).ToList();
        List<BookResponse> books = Books
            .Where(b => completion.Select(x => x.BookId).Contains(b.BookId))
            .Select(book => book.ToResponse())
            .ToList();

        return new(
            BookshelfType.Completed,
            "Completed",
            books,
            completion.Select(b => new BookCompletionData(b.BookId, b.CompletionDate)).ToList());
    }
}

[Table("InProgressBookshelves")]
public class InProgressBookshelf(string childId) : Bookshelf
{
    [StringLength(22)]
    public string ChildId { get; set; } = childId;
    
    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child Child { get; set; } = null!; // 1:1 - Required reference navigation to principal
    
    public override BookshelfResponse ToResponse(int numBooks = int.MaxValue)
    {
        return new(
            BookshelfType.InProgress,
            "In Progress",
            Books.Select(book => book.ToResponse()).Take(numBooks).ToList());
    }
}

[Table("ChildBookshelves")]
public class ChildBookshelf(string name, string childId) : Bookshelf
{
    [StringLength(256, ErrorMessage = "Bookshelf name cannot be longer than {0} characters.")] 
    public string Name { get; set; } = name;
    
    [StringLength(22)]
    public string ChildId { get; set; } = childId;

    // Navigation

    [ForeignKey(nameof(ChildId))] 
    public Child Child { get; set; } = null!; // 1:1 - Required reference navigation to principal

    public override BookshelfResponse ToResponse(int numBooks = int.MaxValue)
    {
        return new(
            BookshelfType.Custom,
            Name,
            Books.Select(book => book.ToResponse()).Take(numBooks).ToList());
    }
}

[Table("ClassroomBookshelves")]
public class ClassroomBookshelf(string name, string classroomCode) : Bookshelf
{
    [StringLength(256, ErrorMessage = "Bookshelf name cannot be longer than {0} characters.")] 
    public string Name { get; set; } = name;
    
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Bookshelf classroom code must be exactly {0} characters long.")]
    public string ClassroomCode { get; set; } = classroomCode;

    // Navigation

    [ForeignKey(nameof(ClassroomCode))] 
    public Classroom Classroom { get; set; } = null!;

    public override BookshelfResponse ToResponse(int numBooks = int.MaxValue)
    {
        return new(
            BookshelfType.Classroom,
            Name,
            Books.Select(book => book.ToResponse()).Take(numBooks).ToList());
    }
}