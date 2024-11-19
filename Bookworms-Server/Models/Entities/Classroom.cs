using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Entities;

public class Classroom(string classroomName)
{
    [Key, StringLength(6, MinimumLength = 6, ErrorMessage = "Classroom code must be exactly {0} characters long.")]
    public string ClassroomCode { get ; set; } = GenerateClassroomCode();

    [StringLength(256, ErrorMessage = "Classroom name cannot be longer than {0} characters.")]
    public string ClassroomName { get; set; } = classroomName;
    
    // Navigation
    
    public Teacher? Teacher { get; set; }
    public ICollection<Child> Children { get; set; } = null!;
    public ICollection<ClassroomBookshelf> Bookshelves { get; set; } = null!;

    private static string GenerateClassroomCode()
    {
        return "ABC123"; // TODO - Generate random string with no collisions
    }
}
