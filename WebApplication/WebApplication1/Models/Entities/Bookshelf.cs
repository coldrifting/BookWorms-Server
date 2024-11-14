using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Entities;

public class Bookshelf(string name)
{
    [Key]
    public Guid BookshelfId { get; set; }

    [StringLength(256)] 
    public string Name { get; set; } = name;
    
    // Navigation

    public virtual ICollection<BookshelfBook>? Books { get; set; }
    public virtual ICollection<BookshelfChild>? ChildBookshelves { get; set; }
    public virtual ICollection<BookshelfClassroom>? ClassroomBookshelves { get; set; }
}