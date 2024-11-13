using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Parent
{
    [Key, StringLength(64, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 5)]
    public string Username { get; set; } = null!;
    
    [ForeignKey(nameof(Username))]
    public User User { get; set; } = null!;
    
    public virtual ICollection<Child>? Children { get; set; }
}