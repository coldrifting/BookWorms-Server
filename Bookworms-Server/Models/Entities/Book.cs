using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;

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

    public bool LevelIsLocked { get; set; }

    [Range(0.0, 5.0, ErrorMessage = "Star rating must be between {0} and {1}.")]
    public double? StarRating { get; set; }
    
    // This is only to have parity with how the data retriever script works 
    [Column(TypeName = "datetime(0)")]
    public DateTime? TimeAdded { get; set; }
    
    // Navigation
    
    public ICollection<Review> Reviews { get; set; } = null!;
    public ICollection<DifficultyRating> DifficultyRatings { get; set; } = null!;
    
    public ICollection<CompletedBookshelfBook> CompletedBookshelfBooks { get; set; } = null!;
    public ICollection<InProgressBookshelfBook> InProgressBookshelfBooks { get; set; } = null!;
    public ICollection<ChildBookshelfBook> ChildBookshelfBooks { get; set; } = null!;
    public ICollection<ClassroomBookshelfBook> ClassroomBookshelfBooks { get; set; } = null!;
    
    // Skip-navigations (many-to-many)
    [InverseProperty("Books")] public ICollection<CompletedBookshelf> CompletedBookshelves { get; set; } = null!;
    [InverseProperty("Books")] public ICollection<InProgressBookshelf> InProgressBookshelves { get; set; } = null!;
    [InverseProperty("Books")] public ICollection<ChildBookshelf> ChildBookshelves { get; set; } = null!;
    [InverseProperty("Books")] public ICollection<ClassroomBookshelf> ClassroomBookshelves { get; set; } = null!;

    public void UpdateStarRating()
    {
        const int numDecPlaces = 2;
        StarRating = Reviews.Count > 0 
            ? Math.Round(Reviews.Average(r => r.StarRating), numDecPlaces) 
            : null;
    }
    
    public BookResponsePreview ToResponsePreview()
    {
        return new(
            BookId,
            Title,
            Authors
        );
    }
    
    public BookResponse ToResponse()
    {
        return new(
            BookId,
            Title,
            Authors,
            StarRating,
            Level
        );
    }
    
    public BookResponseExtended ToResponseExtended()
    {
        return new(
            BookId,
            Title,
            Authors,
            StarRating,
            Level,
            Description,
            Subjects,
            Isbn10 == "" ? null : Isbn10,
            Isbn13 == "" ? null : Isbn13,
            PublishYear,
            PageCount,
            Reviews.Select(review => review.ToResponse()).ToList()
        );
    }

    public UpdatedLevelResponse ToUpdatedLevelResponse(int? oldLevel)
    {
        return new(
            "book",
            BookId,
            oldLevel,
            Level
        );
    }

    public void UpdateLevel(BookwormsDbContext dbContext)
    {
        if (LevelIsLocked)
        {
            return;
        }

        if (DifficultyRatings.Count > 0)
        {
            int newLevel = (int)Math.Round(DifficultyRatings.Average(
                r => 3 * (r.Rating - 3) + r.ReadingLevelAtRatingTime));
            newLevel = int.Max(newLevel, 0);
            newLevel = int.Min(newLevel, 100);
            Level = newLevel;
        }
        else
        {
            Level = null;
        }
        
        if (DifficultyRatings.Count >= 200)
        {
            LevelIsLocked = true;
            dbContext.RemoveRange(DifficultyRatings.Take(200).ToList());
        }
    }
}

public static class BookExtensions
{
	public static List<BookResponse> ToResponse(this IEnumerable<Book> books)
	{
		return books.Select(book => book.ToResponse()).ToList();
	}
}