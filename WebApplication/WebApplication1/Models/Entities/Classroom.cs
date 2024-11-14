using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Entities;

public class Classroom(string classroomName)
{
    [Key, StringLength(6, MinimumLength = 6)]
    public string ClassroomCode { get ; init; } = GenerateClassroomCode();

    [StringLength(256)]
    public string ClassroomName { get; set; } = classroomName;
    
    // Navigation
    
    public virtual Teacher? Teacher { get; set; }
    public virtual ICollection<BookshelfClassroom>? BookshelfClassrooms { get; set; }
    public virtual ICollection<Child>? Children { get; set; }

    private static string GenerateClassroomCode()
    {
        return "ABC123"; // TODO - Generate random string with no collisions
    }
}
