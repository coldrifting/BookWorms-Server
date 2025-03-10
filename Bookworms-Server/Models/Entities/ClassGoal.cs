using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;
using BookwormsServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

// Goals Overview

[Table("ClassGoals")]
public abstract class ClassGoal(string classCode, string title, DateOnly startDate, DateOnly endDate)
{
    [Key]
    [StringLength(14)]
    [Column(TypeName="char")]
    public string ClassGoalId { get; set; } = Snowflake.Generate();
    
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Classroom code must be exactly {1} characters long.")]
    public string ClassCode { get; set; } = classCode;
    
    [StringLength(256, ErrorMessage = "Classroom goal title cannot be longer than {0} characters.")]
    public string Title { get; set; } = title;

    public DateOnly StartDate { get; set; } = startDate;
    public DateOnly EndDate { get; set; } = endDate;

    
    // Navigation
    
    [ForeignKey(nameof(ClassCode))] 
    public Classroom Classroom { get; set; } = null!;

    public ICollection<ClassGoalLog> GoalLogs { get; set; } = null!;

    public int NumStudentsCompleted => GoalLogs.Select(log => log.IsGoalCompleted).Count(boolean => boolean);
    public int TotalStudents => Classroom.Children.Count;

    protected List<ClassGoalStudentStatusLine> GetGoalDetails()
    {
        var allDetails = new List<ClassGoalStudentStatusLine>();
        var loggedChildrenIds = GoalLogs.Select(s => s.ChildId);
        foreach (Child child in Classroom.Children.Where(c => !loggedChildrenIds.Contains(c.ChildId)))
        {
            allDetails.Add(new(child.Name, child.ChildIcon, false));
        }
        
        List<ClassGoalStudentStatusLine> completedLogs = GoalLogs.Select(s => s.ToResponse()).ToList();

        allDetails.AddRange(completedLogs);
        allDetails.Sort((c1, c2) => string.Compare(c1.ChildName, c2.ChildName, StringComparison.Ordinal));
        return allDetails;
    }
}

public static class ClassGoalExt
{
    public static List<ClassGoalTeacherResponse> ToTeacherResponse(this IEnumerable<ClassGoal> goals)
    {
        List<ClassGoalTeacherResponse> classGoalResponses = [];

        foreach (ClassGoal classGoal in goals)
        {
            classGoalResponses.Add(classGoal.ToTeacherResponse());
        }

        return classGoalResponses;
    }
    
    public static ClassGoalTeacherResponse ToTeacherResponse(this ClassGoal classGoal)
    {
        return classGoal switch
        {
            ClassGoalCompletion completion => completion.ToTeacherResponse(),
            ClassGoalNumBooks numBooks => numBooks.ToTeacherResponse(),
            _ => throw new ArgumentException()
        };
    }
    
    public static ClassGoalDetailedTeacherResponse ToTeacherResponseFull(this ClassGoal classGoal)
    {
        return classGoal switch
        {
            ClassGoalCompletion completion => completion.ToTeacherResponseFull(),
            ClassGoalNumBooks numBooks => numBooks.ToTeacherResponseFull(),
            _ => throw new ArgumentException()
        };
    }

    public static GenericGoalChildResponse ToChildResponse(this ClassGoal classGoal, string childId)
    {
        return classGoal switch
        {
            ClassGoalCompletion completion => completion.ToChildResponse(childId),
            ClassGoalNumBooks numBooks => numBooks.ToChildResponse(childId),
            _ => throw new ArgumentException()
        };
    }
}

public class ClassGoalCompletion(string classCode, string title, DateOnly startDate, DateOnly endDate)
    : ClassGoal(classCode, title, startDate, endDate)
{
    private int? AverageCompletionTime
    {
        get
        {
            int total = 0;
            int count = 0;
            foreach (var log in GoalLogs.ToList())
            {
                if (log is ClassGoalLogCompletion { Progress: >= 1f } completionLog)
                {
                    count++;
                    total += completionLog.Duration;
                }
            }

            if (count > 0)
            {
                return total / count;
            }

            return null;
        }
    }

    public ClassGoalTeacherResponse ToTeacherResponse()
    {
        return new(
            ClassGoalId,
            Title,
            StartDate,
            EndDate,
            NumStudentsCompleted,
            TotalStudents,
            new(AverageCompletionTime),
            null);
    }

    public ClassGoalDetailedTeacherResponse ToTeacherResponseFull()
    {
        return new(
            ClassGoalId,
            Title,
            StartDate,
            EndDate,
            NumStudentsCompleted,
            TotalStudents,
            new(AverageCompletionTime),
            null,
            GetGoalDetails());
    }

    public ClassGoalCompletionChildResponse ToChildResponse(string childId)
    {
        ClassGoalLog? childLog = GoalLogs.FirstOrDefault(l => l.ChildId == childId);
        return new(
            ClassGoalId,
            Title,
            Classroom.ClassroomName,
            ClassCode,
            StartDate,
            EndDate,
            (childLog as ClassGoalLogCompletion)?.Progress ?? 0f,
            (childLog as ClassGoalLogCompletion)?.Duration ?? 0);
    }
}

public class ClassGoalNumBooks(string classCode, string title, DateOnly startDate, DateOnly endDate, int targetNumBooks)
    : ClassGoal(classCode, title, startDate, endDate)
{
    public int TargetNumBooks { get; set; } = targetNumBooks;

    private double? AverageBooksRead => GoalLogs.Count > 0
        ? GoalLogs.Average(goalLog => goalLog is ClassGoalLogNumBooks x ? x.NumBooks : 0)
        : null;

    public ClassGoalTeacherResponse ToTeacherResponse()
    {
        return new(
            ClassGoalId,
            Title,
            StartDate,
            EndDate,
            NumStudentsCompleted,
            TotalStudents,
            null,
            new(TargetNumBooks, AverageBooksRead));
    }

    public ClassGoalDetailedTeacherResponse ToTeacherResponseFull()
    {
        return new(
            ClassGoalId,
            Title,
            StartDate,
            EndDate,
            NumStudentsCompleted,
            TotalStudents,
            null,
            new(TargetNumBooks, AverageBooksRead),
            GetGoalDetails());
    }
    
    public ClassGoalNumBooksChildResponse ToChildResponse(string childId)
    {
        ClassGoalLog? childLog = GoalLogs.FirstOrDefault(l => l.ChildId == childId);
        return new(
            ClassGoalId,
            Title,
            Classroom.ClassroomName,
            ClassCode,
            StartDate,
            EndDate,
            TargetNumBooks,
            (childLog as ClassGoalLogNumBooks)?.NumBooks ?? 0);
    }
}

// Logs

[Table("ClassGoalLogs")]
[PrimaryKey(nameof(ClassGoalId), nameof(ChildId))]
public abstract class ClassGoalLog
{
    protected ClassGoalLog()
    {
        
    }

    protected ClassGoalLog(string classGoalId, string classCode, string childId)
    {
        ClassGoalId = classGoalId;
        ClassCode = classCode;
        ChildId = childId;
    }

    [StringLength(14)]
    [Column(TypeName = "char")]
    public string ClassGoalId { get; set; } = null!;
    
    // This lets us ensure that only children enrolled in the class can add goals,
    // and that those goals will be deleted if they leave the class
    [StringLength(6)]
    public string ClassCode { get; set; } = null!;
    
    [StringLength(14)]
    public string ChildId { get; set; } = null!;
    
    // Navigation

    [ForeignKey(nameof(ClassGoalId))]
    public ClassGoal ClassGoal { get; set; } = null!;

    [ForeignKey(nameof(ClassCode) + "," + nameof(ChildId))]
    public ClassroomChild ClassroomChild { get; set; } = null!;
    
    [NotMapped]
    public abstract bool IsGoalCompleted { get; }
    
    public abstract ClassGoalStudentStatusLine ToResponse();
}

public static class ClassGoalLogExt
{
    public static ClassGoalLog? ToClassGoalLog(this GoalProgressUpdateRequest request, ClassGoal goal, string childId)
    {
        if (goal is ClassGoalCompletion goalCompletion)
        {
            if (request is { Progress: { } progress, Duration: { } duration })
            {
                return new ClassGoalLogCompletion(
                    goalCompletion.ClassGoalId,
                    goalCompletion.ClassCode,
                    childId,
                    progress,
                    duration);
            }
        } 
        
        if (goal is ClassGoalNumBooks goalNumBooks)
        {
            if (request.NumBooks is { } numBooks)
            {
                return new ClassGoalLogNumBooks(
                    goalNumBooks.ClassGoalId,
                    goalNumBooks.ClassCode,
                    childId,
                    numBooks);
            }
        }

        return null;
    }
}

public class ClassGoalLogCompletion : ClassGoalLog
{
    public ClassGoalLogCompletion()
    {
        
    }
    
    public ClassGoalLogCompletion(string classGoalId, string classCode, string childId, float progress, int duration) : base(classGoalId, classCode, childId)
    {
        Progress = progress;
        Duration = duration;
    }
    
    public float Progress { get; set; }
    public int Duration { get; set; }

    public override ClassGoalStudentStatusLine ToResponse()
    {
        return new(ClassroomChild.Child.Name, 
            ClassroomChild.Child.ChildIcon, 
            IsGoalCompleted);
    }

    [NotMapped]
    public override bool IsGoalCompleted => Progress >= 1f;
}

public class ClassGoalLogNumBooks : ClassGoalLog
{
    public ClassGoalLogNumBooks()
    {
        
    }
    
    public ClassGoalLogNumBooks(string classGoalId, string classCode, string childId, int numBooks) : base(classGoalId, classCode, childId)
    {
        NumBooks = numBooks;
    }

    public int NumBooks { get; set; }

    public override ClassGoalStudentStatusLine ToResponse()
    {
        return new(
            ClassroomChild.Child.Name, 
            ClassroomChild.Child.ChildIcon, 
            IsGoalCompleted);
    }
    
    [NotMapped]
    public override bool IsGoalCompleted => ClassGoal is ClassGoalNumBooks x && NumBooks >= x.TargetNumBooks;
}
