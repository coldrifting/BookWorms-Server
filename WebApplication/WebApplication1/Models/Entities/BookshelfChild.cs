using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models.Entities;

[PrimaryKey(nameof(BookshelfId), nameof(ChildId))]
public class BookshelfChild(Guid bookshelfId, Guid childId)
{
    public Guid BookshelfId { get; set; } = bookshelfId;
    public Guid ChildId { get; set; } = childId;

    // Navigation
    
    [ForeignKey(nameof(BookshelfId))] 
    public virtual Bookshelf? Bookshelf { get; set; }
    
    [ForeignKey(nameof(ChildId))] 
    public virtual Child? Child { get; set; }
}