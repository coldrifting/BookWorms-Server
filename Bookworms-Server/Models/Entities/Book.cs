using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookwormsServer.Models.Entities;

[Table("Books")]
public class Book(string bookId, string isbn, string title, List<string> authors)
{
    [Key]
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    [StringLength(14)] 
    public string Isbn { get; set; } = isbn;
    
    [StringLength(256, ErrorMessage = "Book title cannot be longer than {0} characters.")]
    public string Title { get; set; } = title;
    
    public List<string> Authors { get; set; } = authors;
    
    [StringLength(16)]
    public string? Level { get; set; }

    [Range(0, 5, ErrorMessage = "Star rating must be between {0} and {1}.")]
    public double? StarRating { get; set; }
    
    // Navigation
    
    public ICollection<Review> Reviews { get; set; } = null!;
    public ICollection<BookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Bookshelf> Bookshelves { get; set; } = null!; // Skip-navigation (many-to-many)
}
