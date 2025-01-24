using System.ComponentModel.DataAnnotations;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record ChildEditDTO(
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    string? NewName = null,

    string? ChildIcon = null,

    [StringLength(6)] string? ReadingLevel = null,

    [StringLength(6, MinimumLength = 6, ErrorMessage = "Child classroom code must be exactly {1} characters long.")]
    string? ClassroomCode = null,

    [Range(typeof(DateOnly), "01/01/1900", "01/01/2100",
        ErrorMessage = "Child date of birth must fall between {1} and {2}.")]
    DateOnly? DateOfBirth = null);

public record ChildResponseDTO(Guid ChildId, string Name, string ChildIcon, string? ReadingLevel, string? ClassroomCode, DateOnly? DateOfBirth, bool? Selected)
{
    public static ChildResponseDTO From(Child child, bool? selected = null)
    {
        return new(child.ChildId, child.Name, child.ChildIcon.ToString(), child.ReadingLevel, child.ClassroomCode, child.DateOfBirth, selected);
    }
}