using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

/*
 * By default, EF takes a "Table-Per-Hierarchy" approach to inheritance.
 * That means that one table will be created for BookshelfBook and all its subclasses,
 * with an automatically created Discriminator column to distinguish between entities
 * of different types.
 */

[Table("BookshelfBooks")]
[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class BookshelfBook(Guid bookshelfId, string bookId)
{
    [ForeignKey("Bookshelf")]
    public Guid BookshelfId { get; set; } = bookshelfId;
    
    [ForeignKey("Book")]
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    // Navigation
    
    public Bookshelf? Bookshelf { get; set; }
    public Book? Book { get; set; }
}

public class CompletedBookshelfBook(Guid bookshelfId, string bookId, double starRating)
    : BookshelfBook(bookshelfId, bookId)
{
    [Range(0, 5, ErrorMessage = "Star rating must be between {0} and {1}.")]
    public double StarRating { get; set; } = starRating;
}