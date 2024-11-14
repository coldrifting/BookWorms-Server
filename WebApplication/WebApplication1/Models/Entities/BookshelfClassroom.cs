using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models.Entities;

[PrimaryKey(nameof(BookshelfId), nameof(ClassroomCode))]
public class BookshelfClassroom(Guid bookshelfId, string classroomCode)
{
    public Guid BookshelfId { get; set; } = bookshelfId;
    
    [StringLength(6)]
    public string ClassroomCode { get; set; } = classroomCode;

    // Navigation
    
    [ForeignKey(nameof(BookshelfId))] 
    public virtual Bookshelf? Bookshelf { get; set; }
    
    [ForeignKey(nameof(ClassroomCode))] 
    public virtual Classroom? Classroom { get; set; }
}