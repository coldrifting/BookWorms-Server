using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities;

[Table("Books")]
public class Book(string isbn, string title, string author, string level)
{
    [Key]
    public Guid BookId { get; set; }
    
    [StringLength(14)] 
    public string Isbn { get; set; } = isbn;
    
    [StringLength(256, ErrorMessage = "Book title cannot be longer than {0} characters.")]
    public string Title { get; set; } = title;
    
    [StringLength(256, ErrorMessage = "Author name cannot be longer than {0} characters.")]
    public string Author { get; set; } = author;
    
    [StringLength(16)]
    public string Level { get; set; } = level;

    [Range(0, 5, ErrorMessage = "Star rating must be between {0} and {1}.")]
    public double StarRating { get; set; }
    
    // Navigation
    
    public ICollection<Review> Reviews { get; set; } = null!;
    public ICollection<BookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Bookshelf> Bookshelves { get; set; } = null!; // Skip-navigation (many-to-many)
}
