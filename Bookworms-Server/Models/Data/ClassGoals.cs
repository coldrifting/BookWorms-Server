using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Data;

public record ClassGoalAddRequest(
    string Title,
    DateOnly EndDate,
    [Range(1, int.MaxValue, ErrorMessage = "{0} must be a greater than 0")]
    int? TargetNumBooks = 2);

public record ClassGoalEditRequest(
    string? NewTitle,
    DateOnly? NewEndDate,
    [Range(1, int.MaxValue, ErrorMessage = "{0} must be a greater than 0")]
    int? NewTargetNumBooks = 2);

public record ClassGoalLogEditRequest(
    float? Progress = null,
    int? Duration = null,
    int? NumBooks = null);

public record ClassGoalResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents);

public record ClassGoalCompletionResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int? AverageCompletionTime)
    : ClassGoalResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents);

public record ClassGoalNumBooksResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int TargetNumBooks,
    double? AverageBooksRead)
    : ClassGoalResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents);

public record ClassGoalStudentStatusLine(
    string ChildName,
    int ChildIcon,
    bool HasAchievedGoal);

public record ClassGoalOverviewResponse(
    List<ClassGoalCompletionResponse> CompletionGoals,
    List<ClassGoalNumBooksResponse> NumBookGoals);

public record ClassGoalDetailedResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    List<ClassGoalStudentStatusLine> StudentGoalStatus)
    : ClassGoalResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents);

public record ClassGoalNumBooksDetailedResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int TargetNumBooks,
    double? AverageBooksRead,
    List<ClassGoalStudentStatusLine> StudentGoalStatus)
    : ClassGoalDetailedResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents,
        StudentGoalStatus);

public record ClassGoalCompletionDetailedResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int? AverageCompletionTime,
    List<ClassGoalStudentStatusLine> StudentGoalStatus)
    : ClassGoalDetailedResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents,
        StudentGoalStatus);
    