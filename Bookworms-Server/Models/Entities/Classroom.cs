using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using BookwormsServer.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

[Index(nameof(TeacherUsername), IsUnique = true)]
public class Classroom(string teacherUsername, string classroomName, string? classroomCode = null)
{
    [Key, StringLength(6, MinimumLength = 6, ErrorMessage = "Classroom code must be exactly {1} characters long.")]
    public string ClassroomCode { get ; set; } = classroomCode ?? GenerateClassroomCode();
    
    [StringLength(64, MinimumLength = 5, ErrorMessage = "User username must be between {2} and {1} characters long.")]
    public string TeacherUsername { get; set; } = teacherUsername;

    [StringLength(256, ErrorMessage = "Classroom name cannot be longer than {0} characters.")]
    public string ClassroomName { get; set; } = classroomName;

    public int ClassIcon { get; set; } = 0;

    // Navigation
    [ForeignKey(nameof(TeacherUsername))] 
    public Teacher Teacher { get; set; } = null!;
    
    public ICollection<ClassGoal> Goals { get; set; } = null!;
    public ICollection<ClassroomBookshelf> Bookshelves { get; set; } = null!;
    
    // Skip-navigation (many-to-many)
    public ICollection<Child> Children { get; set; } = null!;

    public static string GenerateClassroomCodeWithCheck(List<string> existingCodes)
    {
        string code = GenerateClassroomCode();
        while (existingCodes.Contains(code))
        {
            code = GenerateClassroomCode();
        }

        return code;
    }
    
    private static string GenerateClassroomCode()
    {
        StringBuilder builder = new();
        
        for (int i = 0; i < 3; i++)
        {
            builder.Append((char)(Random.Shared.Next(26) + 'A'));
        }
        
        for (int i = 0; i < 3; i++)
        {
            builder.Append(Random.Shared.Next(9));
        }

        return builder.ToString();
    }

    public ClassroomTeacherResponse ToResponseTeacher()
    {
        return new(
            ClassroomCode,
            ClassroomName,
            ClassIcon,
            Children.Select(child => child.ToResponse()).ToList(),
            Bookshelves.Select(bookshelf => bookshelf.ToResponse()).ToList());
    }

    public ClassroomChildResponse ToResponseChild()
    {
        return new(
            ClassroomCode,
            ClassroomName,
            Teacher.LastName,
            ClassIcon,
            Bookshelves.Select(bookshelf => bookshelf.ToResponse()).ToList());
    }
}
