using System.ComponentModel.DataAnnotations;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public class AddChildDTO(string name)
{
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    public string Name { get; set; } = name;
}

public class ChildEditDTO(string? newName, string? readingLevel, string? classroomCode, DateOnly? dateOfBirth)
{
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    public string? NewName { get; set; } = newName;
    
    [StringLength(6)]
    public string? ReadingLevel { get; set; } = readingLevel;
    
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Child classroom code must be exactly {0} characters long.")]
    public string? ClassroomCode { get; set; } = classroomCode;
    
    [Range(typeof(DateOnly), "01/01/1900", "01/01/2100",
        ErrorMessage = "Child date of birth must fall between {1} and {2}.")]
    public DateOnly? DateOfBirth { get; set; } = dateOfBirth;
}

public record ChildResponseDTO(string Name, string? ReadingLevel, DateOnly? DateOfBirth)
{
    public static ChildResponseDTO From(Child child)
    {
        return new(child.Name, child.ReadingLevel, child.DateOfBirth);
    }
}