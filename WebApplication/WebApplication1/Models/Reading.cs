using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

[PrimaryKey(nameof(ChildId), nameof(BookId))]
public class Reading
{
    public Guid ChildId { get; set; }
    public Guid BookId { get; set; }
    
    [ForeignKey("ChildId")]
    public virtual Child? Child { get; set; }
}