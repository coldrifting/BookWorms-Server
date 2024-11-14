using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models.Entities;

public class Review(Guid bookId, string username, string details, int rating)
{
    [Key]
    public Guid ReviewId { get; set; }

    public Guid BookId { get; set; } = bookId;

    [StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = username;

    [StringLength(4096)]
    public string Details { get; set; } = details;

    public int Rating { get; set; } = rating;
    
    // Navigation
    
    [ForeignKey(nameof(BookId))] 
    public virtual Book? Book { get; set; }

    [ForeignKey(nameof(Username))] 
    public virtual User? Reviewer { get; set; }

}