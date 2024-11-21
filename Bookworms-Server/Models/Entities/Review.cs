using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookwormsServer.Models.Entities;

[Table("Reviews")]
public class Review(string bookId, string username, double starRating, string? reviewText)
{
    [Key]
    public Guid ReviewId { get; set; }

    [StringLength(20)]
    public string BookId { get; set; } = bookId;

    [StringLength(64, MinimumLength = 5, ErrorMessage = "Reviewer username must be between {2} and {1} characters long.")]
    public string Username { get; set; } = username;

    [Range(0, 5, ErrorMessage = "Star rating must be between {1} and {2}.")]
    public double StarRating { get; set; } = starRating;

    [StringLength(4096, ErrorMessage = "Review text must be shorter than {0} characters.")]
    public string? ReviewText { get; set; } = reviewText;
    
    // Navigation
    
    [ForeignKey(nameof(BookId))] 
    public Book? Book { get; set; }

    [ForeignKey(nameof(Username))] 
    public User? Reviewer { get; set; }
}