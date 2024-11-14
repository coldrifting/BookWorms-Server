using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models.Entities;

[PrimaryKey(nameof(ChildId), nameof(BookId))]
public class Completed
{
    public Guid ChildId { get; set; }
    public Guid BookId { get; set; }
    public int Rating { get; set; }
        
    // Navigation
    
    [ForeignKey(nameof(ChildId))]
    public virtual Child? Child { get; set; }
    
    [ForeignKey(nameof(BookId))]
    public virtual Book? Book { get; set; }
}