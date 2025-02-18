using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

[Table("DifficultyRatings")]
[PrimaryKey(nameof(BookId), nameof(ChildId))]
public class DifficultyRating(string bookId, string childId, int readingLevelAtRatingTime, int rating)
{
    [StringLength(20)]
    public string BookId { get; set; } = bookId;
    
    [StringLength(22)]
    public string ChildId { get; set; } = childId;
    
    [Range(0, 100, ErrorMessage = "Child reading level must be between {1} and {2}.")]
    public int ReadingLevelAtRatingTime { get; set; } = readingLevelAtRatingTime;

    public int Rating { get; set; } = rating;

    // Navigation
    
    [ForeignKey(nameof(BookId))] 
    public Book? Book { get; set; }
    
    [ForeignKey(nameof(ChildId))] 
    public Child? Child { get; set; }
}