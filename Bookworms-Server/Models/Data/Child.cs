using System.ComponentModel.DataAnnotations;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public class ChildEditDTO(string? newName = null, string? readingLevel = null, string? classroomCode = null, DateOnly? dateOfBirth = null)
{
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    public string? NewName { get; set; } = newName;
    
    [StringLength(6)]
    public string? ReadingLevel { get; set; } = readingLevel;
    
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Child classroom code must be exactly {1} characters long.")]
    public string? ClassroomCode { get; set; } = classroomCode;
    
    [Range(typeof(DateOnly), "01/01/1900", "01/01/2100",
        ErrorMessage = "Child date of birth must fall between {1} and {2}.")]
    public DateOnly? DateOfBirth { get; set; } = dateOfBirth;
}

public record ChildResponseDTO(string Name, string? ReadingLevel, string? ClassroomCode, DateOnly? DateOfBirth, bool? Selected)
{
    public static ChildResponseDTO From(Child child, bool? selected = null)
    {
        return new(child.Name, child.ReadingLevel, child.ClassroomCode, child.DateOfBirth, selected);
    }
}