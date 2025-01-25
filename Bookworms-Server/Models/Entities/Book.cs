using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookwormsServer.Models.Entities;

[Table("Books")]
public class Book(string bookId, string title, List<string> authors, string description, string isbn10, string isbn13)
{
    [Key]
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    [StringLength(256, ErrorMessage = "Book title cannot be longer than {0} characters.")]
    public string Title { get; set; } = title;
    
    public List<string> Authors { get; set; } = authors;
    
    // Intentionally unconstrained (nvarchar(max)); descriptions are lengthy
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Description { get; set; } = description;
    
    public List<string> Subjects { get; set; } = [];
    
    [StringLength(10)] 
    public string Isbn10 { get; set; } = isbn10;
    
    [StringLength(13)] 
    public string Isbn13 { get; set; } = isbn13;
    
    [Range(0, int.MaxValue, ErrorMessage = "Cover ID must be non-negative.")]
    public int? CoverId { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Page count must be non-negative.")]
    public int? PageCount { get; set; }
    
    [Range(0, 2025, ErrorMessage = "Publish year must be an A.D. date before 2026.")]
    public int PublishYear { get; set; }
    
    [Range(0, 100, ErrorMessage = "Level must be in [0, 100]")]
    public int? Level { get; set; }

    [Range(0, 5, ErrorMessage = "Star rating must be between {0} and {1}.")]
    public double? StarRating { get; set; }
    
    // Navigation
    
    public ICollection<Review> Reviews { get; set; } = null!;
    public ICollection<BookshelfBook> BookshelfBooks { get; set; } = null!;
    public ICollection<Bookshelf> Bookshelves { get; set; } = null!; // Skip-navigation (many-to-many)
}
