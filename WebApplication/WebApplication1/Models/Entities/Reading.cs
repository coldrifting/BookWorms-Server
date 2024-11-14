using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models.Entities;

[PrimaryKey(nameof(ChildId), nameof(BookId))]
public class Reading
{
    public Guid ChildId { get; set; }
    public Guid BookId { get; set; }
    
    // Navigation
    
    [ForeignKey(nameof(ChildId))]
    public virtual Child? Child { get; set; }
    
    [ForeignKey(nameof(BookId))]
    public virtual Book? Book { get; set; }
}