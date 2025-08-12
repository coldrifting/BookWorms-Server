namespace BookwormsServer.Models.Data;

public enum GoalType
{
    Child,
    Classroom,
    ClassroomAggregate
}

public enum GoalMetric
{
    BooksRead,
    MinutesRead,
    Completion
}

public record GoalAddRequest(
    GoalType GoalType,
    GoalMetric GoalMetric,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int Target);

public record GoalEditRequest(
    GoalMetric? GoalMetric = null,
    string? Title = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    int? Target = null);

public record GoalResponse(
    string GoalId,
    GoalType GoalType,
    GoalMetric GoalMetric,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    int Target,
    int Progress, // Average for classroom goals
    ClassGoalDetails? ClassGoalDetails = null);

public record ClassGoalDetails(
    int StudentsTotal,
    int StudentsCompleted,
    List<StudentGoalDetails>? StudentGoalDetails = null);
    
public record StudentGoalDetails(
    string Name,
    int Icon,
    int Progress);