using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Entities;

public class Book(string isbn, string title, string author, string level)
{
    [Key]
    public Guid BookId { get; set; }
    
    [StringLength(14)] 
    public string Isbn { get; set; } = isbn;
    
    public string Title { get; set; } = title;
    public string Author { get; set; } = author;
    public string Level { get; set; } = level;

    public int Rating { get; set; }
    
    // Navigation
    public virtual ICollection<Completed>? Completeds { get; set; }
    public virtual ICollection<Reading>? Readings { get; set; }

    public virtual ICollection<Review>? Reviews { get; set; }
    public virtual ICollection<BookshelfBook>? Bookshelves { get; set; }
}