using System.ComponentModel.DataAnnotations;

namespace BookwormsServer.Models.Data;


// REQUESTS

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


// TEACHER RESPONSES

public record ClassGoalOverviewTeacherResponse(
    List<ClassGoalCompletionTeacherResponse> CompletionGoals,
    List<ClassGoalNumBooksTeacherResponse> NumBookGoals);

public record ClassGoalTeacherResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents);

public record ClassGoalCompletionTeacherResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int? AverageCompletionTime)
    : ClassGoalTeacherResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents);

public record ClassGoalNumBooksTeacherResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int TargetNumBooks,
    double? AverageBooksRead)
    : ClassGoalTeacherResponse(
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

public record ClassGoalDetailedTeacherResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    List<ClassGoalStudentStatusLine> StudentGoalStatus)
    : ClassGoalTeacherResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents);

public record ClassGoalCompletionDetailedTeacherResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int? AverageCompletionTime,
    List<ClassGoalStudentStatusLine> StudentGoalStatus)
    : ClassGoalDetailedTeacherResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents,
        StudentGoalStatus);

public record ClassGoalNumBooksDetailedTeacherResponse(
    string GoalId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int StudentsCompleted,
    int TotalStudents,
    int TargetNumBooks,
    double? AverageBooksRead,
    List<ClassGoalStudentStatusLine> StudentGoalStatus)
    : ClassGoalDetailedTeacherResponse(
        GoalId,
        Title,
        StartDate,
        EndDate,
        StudentsCompleted,
        TotalStudents,
        StudentGoalStatus);


// CHILD RESPONSES

public record ClassGoalCompletionChildResponse(
    string GoalId,
    string Title,
    string ClassName,
    string ClassCode,
    DateOnly StartDate,
    DateOnly EndDate,
    float Progress,
    int Duration)
    : GenericGoalChildResponse(
        GoalId,
        GoalType.Classroom,
        Title,
        StartDate,
        EndDate);

public record ClassGoalNumBooksChildResponse(
    string GoalId,
    string Title,
    string ClassName,
    string ClassCode,
    DateOnly StartDate,
    DateOnly EndDate,
    int TargetNumBooks,
    int NumBooks)
    : GenericGoalChildResponse(
        GoalId,
        GoalType.Classroom,
        Title,
        StartDate,
        EndDate);