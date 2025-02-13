using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

[Table("CompletedBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class CompletedBookshelfBook(int bookshelfId, string bookId, double starRating)
{    
    public int BookshelfId { get; set; } = bookshelfId;
    
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    [Range(0, 5, ErrorMessage = "Star rating must be between {0} and {1}.")]
    public double StarRating { get; set; } = starRating;
    
    // Navigation
    
    [ForeignKey("BookshelfId")]
    public CompletedBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}

[Table("InProgressBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class InProgressBookshelfBook(int bookshelfId, string bookId)
{
    public int BookshelfId { get; set; } = bookshelfId;
    
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    // Navigation
    
    [ForeignKey("BookshelfId")]
    public InProgressBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}

[Table("ChildBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class ChildBookshelfBook(int bookshelfId, string bookId)
{
    public int BookshelfId { get; set; } = bookshelfId;
    
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    // Navigation
    
    [ForeignKey("BookshelfId")]
    public ChildBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}

[Table("ClassroomBookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class ClassroomBookshelfBook(int bookshelfId, string bookId)
{
    public int BookshelfId { get; set; } = bookshelfId;
    
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    // Navigation
    
    [ForeignKey("BookshelfId")]
    public ClassroomBookshelf? Bookshelf { get; set; }
    
    [ForeignKey("BookId")]
    public Book? Book { get; set; }
}