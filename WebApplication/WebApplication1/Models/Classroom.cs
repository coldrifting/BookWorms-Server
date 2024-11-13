using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Classroom
{
    [Key, StringLength(6)]
    public string ClassroomCode { get; set; } = null!;

    [StringLength(256)]
    public string ClassroomName { get; set; } = null!;
    
    public virtual Teacher? Teacher { get; set; }
}