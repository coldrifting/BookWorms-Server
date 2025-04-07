using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

[Table("CompletedBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class CompletedBookshelfBook
{
    public CompletedBookshelfBook()
    {
    }

    public CompletedBookshelfBook(int bookshelfId, string bookId, DateOnly? completionDate = null)
    {
        BookshelfId = bookshelfId;
        BookId = bookId;
        CompletionDate = completionDate ?? DateOnly.FromDateTime(DateTime.Now);
    }
    
    public int BookshelfId { get; set; }
    
    [StringLength(20)]
    public required string BookId { get; set; }

    public DateOnly CompletionDate { get; set; }

    // Navigation
    
    [ForeignKey("BookshelfId")]
    public CompletedBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}

[Table("InProgressBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class InProgressBookshelfBook
{
    public InProgressBookshelfBook()
    {
    }

    public InProgressBookshelfBook(int bookshelfId, string bookId)
    {
        BookshelfId = bookshelfId;
        BookId = bookId;
    }
    
    public int BookshelfId { get; set; }
    
    [StringLength(20)]
    public required string BookId { get; set; }
    
    // Navigation
    
    [ForeignKey("BookshelfId")]
    public InProgressBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}

[Table("ChildBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class ChildBookshelfBook
{
    public ChildBookshelfBook()
    {
    }

    public ChildBookshelfBook(int bookshelfId, string bookId)
    {
        BookshelfId = bookshelfId;
        BookId = bookId;
    }

    public int BookshelfId { get; set; }
    
    [StringLength(20)]
    public required string BookId { get; set; }
    
    // Navigation
    
    [ForeignKey("BookshelfId")]
    public ChildBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}

[Table("ClassroomBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class ClassroomBookshelfBook
{
    public ClassroomBookshelfBook()
    {
    }

    public ClassroomBookshelfBook(int bookshelfId, string bookId)
    {
        BookshelfId = bookshelfId;
        BookId = bookId;
    }
    
    public int BookshelfId { get; set; }
    
    [StringLength(20)]
    public required string BookId { get; set; }
    
    // Navigation
    
    [ForeignKey("BookshelfId")]
    public ClassroomBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}