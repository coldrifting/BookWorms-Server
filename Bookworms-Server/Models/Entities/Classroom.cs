using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Entities;

public class Classroom(string classroomName, string teacherUsername)
{
    [Key, StringLength(6, MinimumLength = 6, ErrorMessage = "Classroom code must be exactly {1} characters long.")]
    public string ClassroomCode { get ; set; } = GenerateClassroomCode();

    [StringLength(256, ErrorMessage = "Classroom name cannot be longer than {0} characters.")]
    public string ClassroomName { get; set; } = classroomName;

    [StringLength(64, MinimumLength = 5, ErrorMessage = "User username must be between {2} and {1} characters long.")]
    public string TeacherUsername { get; set; } = teacherUsername;

    // Navigation
    public Teacher Teacher { get; set; } = null!;
    public ICollection<Child> Children { get; set; } = null!;
    public ICollection<ClassroomBookshelf> ClassroomBookshelves { get; set; } = null!;
    public ICollection<Book> Books { get; set; } = null!; // Skip-navigation (many-to-many)

    private static string GenerateClassroomCode()
    {
        return "ABC123"; // TODO - Generate random string with no collisions
    }
}
