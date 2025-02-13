using System.ComponentModel.DataAnnotations;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record ChildEditDTO(
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    string? NewName = null,

    [Range(0, int.MaxValue, ErrorMessage = "{0} must be a positive integer.")]
    int? ChildIcon = null,

    [StringLength(6)] string? ReadingLevel = null,

    [StringLength(6, MinimumLength = 6, ErrorMessage = "Child classroom code must be exactly {1} characters long.")]
    string? ClassroomCode = null,

    [Range(typeof(DateOnly), "01/01/1900", "01/01/2100",
        ErrorMessage = "Child date of birth must fall between {1} and {2}.")]
    DateOnly? DateOfBirth = null);

public record ChildResponseDTO(string ChildId, string Name, int? ChildIcon, string? ReadingLevel, string? ClassroomCode, DateOnly? DateOfBirth)
{
    public static ChildResponseDTO From(Child child)
    {
        return new(child.ChildId, child.Name, child.ChildIcon, child.ReadingLevel, child.ClassroomCode, child.DateOfBirth);
    }
}