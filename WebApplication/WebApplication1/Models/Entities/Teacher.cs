using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Entities;

public class Teacher
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = null!;
    
    [StringLength(6)]
    public string? ClassroomCode { get; set; }
    
    // Navigation
    
    [ForeignKey(nameof(Username))]
    public virtual User? User { get; set; }
    
    [ForeignKey(nameof(ClassroomCode))]
    public virtual Classroom? Classroom { get; set; }
}