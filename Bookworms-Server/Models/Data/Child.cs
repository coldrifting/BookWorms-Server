using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Data;

public record ChildEditRequest(
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    string? NewName = null,

    [Range(0, int.MaxValue, ErrorMessage = "{0} must be a positive integer.")]
    int? ChildIcon = null,

    [Range(0, 100, ErrorMessage = "Child reading level must be between {1} and {2}.")]
    int? ReadingLevel = null,

    [Range(typeof(DateOnly), "01/01/1900", "01/01/2100",
        ErrorMessage = "Child date of birth must fall between {1} and {2}.")]
    DateOnly? DateOfBirth = null);

public record ChildResponse(
    string ChildId, 
    string Name, 
    int? ChildIcon, 
    int? ReadingLevel,
    DateOnly? DateOfBirth);