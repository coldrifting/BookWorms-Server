using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Parent
{
    [Key, StringLength(64)]
    public string Username { get; set; } = null!;
    
    [ForeignKey(nameof(Username))]
    public User User { get; set; } = null!;
    
    public virtual ICollection<Child>? Children { get; set; }
}