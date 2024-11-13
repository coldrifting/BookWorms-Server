using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Teacher
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = null!;
    
    [ForeignKey(nameof(Username))]
    public User User { get; set; } = null!;
    
    [StringLength(6)]
    public string? ClassroomCode { get; set; }
    
    [ForeignKey(nameof(ClassroomCode))]
    public Classroom? Classroom { get; set; }
}