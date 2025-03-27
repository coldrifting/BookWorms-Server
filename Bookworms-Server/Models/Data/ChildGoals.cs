using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Data;


// REQUESTS

public record ChildGoalAddRequest(
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    [Range(1, int.MaxValue, ErrorMessage = "{0} must be a greater than 0")]
    int? TargetNumBooks = null);

public record ChildGoalEditRequest(
    string? NewTitle,
    DateOnly? NewStartDate,
    DateOnly? NewEndDate,
    [Range(1, int.MaxValue, ErrorMessage = "{0} must be a greater than 0")]
    int? NewTargetNumBooks = 2);

public record GoalProgressUpdateRequest(
    float? Progress = null,
    int? Duration = null,
    int? NumBooks = null);


// RESPONSES

public enum GoalType
{
    Child,
    Classroom
}

public record AllGoalOverviewResponse(
    List<ChildGoalCompletionResponse> CompletionGoals,
    List<ChildGoalNumBooksResponse> NumBookGoals,
    List<ClassGoalCompletionChildResponse> ClassCompletionGoals,
    List<ClassGoalNumBooksChildResponse> ClassNumBooksGoals);

public record GenericGoalChildResponse(
    string GoalId,
    GoalType Type,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate);

public record ChildGoalCompletionResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    float Progress,
    int Duration)
    : GenericGoalChildResponse(
        GoalId,
        GoalType.Child,
        Title,
        StartDate,
        EndDate);

public record ChildGoalNumBooksResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int TargetNumBooks,
    int NumBooks)
    : GenericGoalChildResponse(
        GoalId,
        GoalType.Child,
        Title,
        StartDate,
        EndDate);
    