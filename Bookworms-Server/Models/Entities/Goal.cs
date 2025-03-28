using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;
using BookwormsServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

[Table("Goals")]
public abstract class Goal(GoalMetric goalMetric, string title, DateOnly startDate, DateOnly endDate, int target)
{
    [Key]
    [StringLength(14)]
    [Column(TypeName="char")]
    public string GoalId { get; set; } = Snowflake.Generate();

    public GoalMetric GoalMetric { get; set; } = goalMetric;
    
    [StringLength(256, ErrorMessage = "Child goal title cannot be longer than {0} characters.")]
    public string Title { get; set; } = title;

    public DateOnly StartDate { get; set; } = startDate;
    public DateOnly EndDate { get; set; } = endDate;

    public int Target { get; set; } = target;
}

// Self-assigned goals
public class GoalChild(
    GoalMetric goalMetric,
    string title,
    DateOnly startDate,
    DateOnly endDate,
    int target,
    string childId) : Goal(goalMetric, title, startDate, endDate, target)
{
    [StringLength(14)]
    [Column(TypeName = "char")]
    public string ChildId { get; set; } = childId;
    
    public int Progress { get; set; }
    
    // Navigation
    [ForeignKey(nameof(ChildId))]
    public Child Child { get; set; } = null!;

    // Helpers
    [NotMapped]
    public bool IsCompleted =>
        GoalMetric == GoalMetric.Completion 
            ? Progress % 1000 >= 100
            : Progress >= Target;

    public GoalResponse ToResponse()
    {
        return new(
            GoalId,
            GoalType.Child,
            GoalMetric,
            Title,
            StartDate,
            EndDate,
            Target,
            Progress);
    }
}

public abstract class GoalClassBase(
    GoalMetric goalMetric,
    string title,
    DateOnly startDate,
    DateOnly endDate,
    int target,
    string classCode) : Goal(goalMetric, title, startDate, endDate, target)
{
    [StringLength(6)]
    [Column(TypeName = "char")]
    public string ClassCode { get; set; } = classCode;
    
    // Navigation
    [ForeignKey(nameof(ClassCode))]
    public Classroom Classroom { get; set; } = null!;
    
    public ICollection<GoalClassLog> Logs { get; set; } = null!;
    
    // Helpers
    [NotMapped]
    public int TotalStudents => Classroom.Children.Count;
    
    public List<StudentGoalDetails> ChildDetails(string childId) => Logs.Where(l => l.ChildId == childId).Select(l => l.ToDetails()).ToList();

    public abstract GoalResponse ToChildResponse(string childId);
    public abstract GoalResponse ToTeacherResponse(bool showDetails = false);
}

// Class assigned goals
public class GoalClass(
    GoalMetric goalMetric,
    string title,
    DateOnly startDate,
    DateOnly endDate,
    int target,
    string classCode) : GoalClassBase(goalMetric, title, startDate, endDate, target, classCode)
{
    // Helpers
    [NotMapped]
    public int CompletedStudents
    {
        get
        {
            return GoalMetric == GoalMetric.Completion 
                ? Logs.Count(l => l.Progress % 1000 >= 100) 
                : Logs.Count(l => l.Progress >= Target);
        }
    }

    [NotMapped]
    public int AverageProgress
    {
        get
        {
            if (TotalStudents == 0)
            {
                return -1;
            }
            
            if (GoalMetric == GoalMetric.Completion)
            {
                int time = Logs.Sum(l => l.Progress / 1000) / TotalStudents;
                int percentage = Logs.Sum(l => l.Progress % 1000) / TotalStudents;
                return time * 1000 + percentage;
            }
            return Logs.Sum(l => l.Progress) / TotalStudents;
        }
    }

    public override GoalResponse ToChildResponse(string childId)
    {
        return new(
            GoalId,
            GoalType.Classroom,
            GoalMetric,
            Title,
            StartDate,
            EndDate,
            Target,
            AverageProgress,
            new(
                TotalStudents,
                CompletedStudents,
                ChildDetails(childId)));
    }
    
    public override GoalResponse ToTeacherResponse(bool showDetails = false)
    {
        return new(
            GoalId,
            GoalType.Classroom,
            GoalMetric,
            Title,
            StartDate,
            EndDate,
            Target,
            AverageProgress,
            new(
                TotalStudents,
                CompletedStudents,
                showDetails ? GetLogs() : null));
    }

    private List<StudentGoalDetails> GetLogs()
    {
        Dictionary<string, StudentGoalDetails> all = Classroom.Children
            .ToDictionary(c => c.ChildId, c => new StudentGoalDetails(c.Name, c.ChildIcon, 0));
        
        Dictionary<string, StudentGoalDetails> logs = Logs
            .ToDictionary(l => l.ChildId, l => l.ToDetails());
        
        logs.ToList().ForEach(kv => all[kv.Key] = kv.Value);

        return all.Values.ToList();
    }
}

// Class Aggregate Goals
public class GoalClassAggregate(
    GoalMetric goalMetric,
    string title,
    DateOnly startDate,
    DateOnly endDate,
    int target,
    string classCode) : GoalClassBase(goalMetric, title, startDate, endDate, target, classCode)
{
    [NotMapped]
    public int Progress => Logs.Sum(l => l.Progress);
    
    public override GoalResponse ToChildResponse(string childId)
    {
        return new(
            GoalId,
            GoalType.ClassroomAggregate,
            GoalMetric,
            Title,
            StartDate,
            EndDate,
            Target,
            Progress,
            new(
                TotalStudents,
                Logs.Count,
                ChildDetails(childId)));
    }
    public override GoalResponse ToTeacherResponse(bool showDetails = false)
    {
        return new(
            GoalId,
            GoalType.ClassroomAggregate,
            GoalMetric,
            Title,
            StartDate,
            EndDate,
            Target,
            Progress,
            new(
                TotalStudents,
                Logs.Count,
                showDetails ? Logs.Select(l => l.ToDetails()).ToList() : null));
    }
}

[Table("GoalClassLogs")]
[PrimaryKey(nameof(GoalId), nameof(ChildId))]
public class GoalClassLog(string goalId, string childId, string classCode, int progress)
{
    [StringLength(14)]
    [Column(TypeName = "char")]
    public string GoalId { get; set; } = goalId;
    
    [StringLength(14)]
    [Column(TypeName = "char")]
    public string ChildId { get; set; } = childId;

    [StringLength(6)]
    [Column(TypeName = "char")]
    public string ClassCode { get; set; } = classCode;

    public int Progress { get; set; } = progress;


    // Navigation
    [ForeignKey(nameof(GoalId))]
    public GoalClassBase GoalClassBase { get; set; } = null!;
    
    [ForeignKey(nameof(ChildId))]
    public Child Child { get; set; } = null!;
    
    // Ensure goal logs are removed when child leaves a class by storing class child table info
    [ForeignKey(nameof(ClassCode) + "," + nameof(ChildId))]
    public ClassroomChild ClassroomChild { get; set; } = null!;

    // Helpers
    [NotMapped]
    public bool IsCompleted =>
        GoalClassBase.GoalMetric == GoalMetric.Completion 
            ? Progress % 1000 >= 100
            : Progress >= GoalClassBase.Target;
    
    public StudentGoalDetails ToDetails()
    {
        return new(Child.Name, Child.ChildIcon, Progress);
    }
}