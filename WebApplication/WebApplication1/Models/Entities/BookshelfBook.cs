using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models.Entities;

[PrimaryKey(nameof(BookshelfId), nameof(BookId))]
public class BookshelfBook
{
    public Guid BookshelfId { get; set; }
    public Guid BookId { get; set; }
    
    // Navigation
    
    [ForeignKey(nameof(BookshelfId))]
    public virtual Bookshelf? Bookshelf { get; set; }
    
    [ForeignKey(nameof(BookId))]
    public virtual Book? Book { get; set; }
}