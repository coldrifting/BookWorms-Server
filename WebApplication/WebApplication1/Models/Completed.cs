using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(ChildId), nameof(BookId))]
public class Completed
{
    public Guid ChildId { get; set; }
    public Guid BookId { get; set; }
    public int Rating { get; set; }
    
    [ForeignKey(nameof(ChildId))]
    public virtual Child? Child { get; set; }
}