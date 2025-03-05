using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;
using BookwormsServer.Utils;

namespace BookwormsServer.Models.Entities;

// Goals Overview

[Table("ChildGoals")]
public abstract class ChildGoal(string childId, string title, DateOnly startDate, DateOnly endDate)
{
    [Key]
    [StringLength(14)]
    [Column(TypeName="char")]
    public string ChildGoalId { get; set; } = Snowflake.Generate();
    
    [StringLength(14)]
    public string ChildId { get; set; } = childId;
    
    [StringLength(256, ErrorMessage = "Child goal title cannot be longer than {0} characters.")]
    public string Title { get; set; } = title;

    public DateOnly StartDate { get; set; } = startDate;
    public DateOnly EndDate { get; set; } = endDate;

    
    // Navigation
    
    [ForeignKey(nameof(ChildId))] 
    public Child Child { get; set; } = null!;
    
    [NotMapped]
    public abstract bool IsCompleted { get; }
}

public static class ChildGoalExt
{
    public static GenericGoalChildResponse ToChildResponse(this ChildGoal childGoal)
    {
        return childGoal switch
        {
            ChildGoalCompletion completion => completion.ToChildResponse(),
            ChildGoalNumBooks numBooks => numBooks.ToChildResponse(),
            _ => throw new ArgumentException()
        };
    }
    
}

public class ChildGoalCompletion(string childId, string title, DateOnly startDate, DateOnly endDate)
    : ChildGoal(childId, title, startDate, endDate)
{
    public float Progress { get; set; }
    public int Duration { get; set; }

    public ChildGoalCompletionResponse ToChildResponse()
    {
        return new(
            ChildGoalId,
            Title,
            StartDate,
            EndDate,
            Progress,
            Duration);
    }
    
    [NotMapped]
    public override bool IsCompleted => Progress >= 1.0f;
}

public class ChildGoalNumBooks(string childId, string title, DateOnly startDate, DateOnly endDate, int targetNumBooks)
    : ChildGoal(childId, title, startDate, endDate)
{
    public int TargetNumBooks { get; set; } = targetNumBooks;
    public int NumBooks { get; set; }

    public ChildGoalNumBooksResponse ToChildResponse()
    {
        return new(
            ChildGoalId,
            Title,
            StartDate,
            EndDate,
            TargetNumBooks,
            NumBooks);
    }
    
    [NotMapped]
    public override bool IsCompleted => NumBooks >= TargetNumBooks;
}