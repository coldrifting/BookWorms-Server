using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;
using BookwormsServer.Utils;

namespace BookwormsServer.Models.Entities;

public class Child(string name, string parentUsername, DateOnly? dateOfBirth = null, int? readingLevel = null)
{
    [Key]
    [StringLength(14)]
    [Column(TypeName="char")]
    public string ChildId { get; set; } = Snowflake.Generate();
    
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Child name must be between {2} and {1} characters long.")]
    public string Name { get; set; } = name;

    [Range(0, int.MaxValue, ErrorMessage = "{0} must be a positive integer.")]
    public int ChildIcon { get; set; }

    [Range(typeof(DateOnly), "01/01/1900", "01/01/2100",
        ErrorMessage = "Child date of birth must fall between {1} and {2}.")]
    public DateOnly? DateOfBirth { get; set; } = dateOfBirth;
    
    [Range(0, 100, ErrorMessage = "Child reading level must be between {1} and {2}.")]
    public int? ReadingLevel { get; set; } = readingLevel ?? CalculateBaseReadingLevel(dateOfBirth);

    [StringLength(64)]
    public string ParentUsername { get; set; } = parentUsername;

    // Navigation

    [ForeignKey(nameof(ParentUsername))] 
    public Parent Parent { get; set; } = null!;
    
    // Skip-navigation (many-to-many)
    [InverseProperty("Children")] public ICollection<Classroom> Classrooms { get; set; } = null!;

    public CompletedBookshelf? Completed { get; set; } // 1:1 - Reference navigation to dependent
    public InProgressBookshelf? InProgress { get; set; } // 1:1 - Reference navigation to dependent
    public ICollection<ChildBookshelf> Bookshelves { get; set; } = null!;
    
    public ICollection<GoalChild> Goals { get; set; } = null!;


    public void AdjustReadingLevel(Book book, int difficultyRating)
    {
        // The controller that calls this method guarantees that both of these are non-null
        int diff = book.Level!.Value - ReadingLevel!.Value;
        int adjustmentOffset = 1 - difficultyRating;

        int newLevel = ReadingLevel.Value;
        switch (diff)
        {
            case < -10: // should be way too easy
                newLevel += adjustmentOffset;
                break;
            case < -5: // should be a bit too easy
                newLevel += 1 + adjustmentOffset;
                break;
            case < 5: // should be just right
                newLevel += 2 + adjustmentOffset;
                break;
            case < 10: // should be a bit too hard
                newLevel += 3 + adjustmentOffset;
                break;
            case < 100: // should be way too hard
                newLevel += 4 + adjustmentOffset;
                break;
        }
        newLevel = int.Max(newLevel, 0);
        newLevel = int.Min(newLevel, 100);
        
        ReadingLevel = newLevel;
    }

    private static int? CalculateBaseReadingLevel(DateOnly? dateOfBirth)
    {
        if (dateOfBirth == null)
        {
            return null;
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly childBirthDate = dateOfBirth.Value;
        
        DateOnly thirdBirthday = childBirthDate.AddYears(3);
        DateOnly eighteenthBirthday = childBirthDate.AddYears(18);
        
        if (thirdBirthday.CompareTo(today) > 0)
            return 0;
        if (eighteenthBirthday.CompareTo(today) < 0)
            return 100;
        
        double fifteenYears = eighteenthBirthday.DayNumber - thirdBirthday.DayNumber;
        double daysPastThree = today.DayNumber - thirdBirthday.DayNumber;
        
        // Percentage of the way between 3 yrs old and 18 yrs old
        return (int?)Math.Round(daysPastThree / fifteenYears * 100);
    }
}

public static class ChildExt
{
    public static ChildResponse ToResponse(this Child child)
    {
        return new(
            child.ChildId,
            child.Name,
            child.ChildIcon,
            child.ReadingLevel,
            child.DateOfBirth);
    }
    
    public static UpdatedLevelResponse ToUpdatedLevelResponse(this Child child, int? oldLevel)
    {
        return new(
            "child",
            child.ChildId,
            oldLevel,
            child.ReadingLevel
        );
    }
}