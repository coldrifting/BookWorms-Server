using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

[Table("ClassroomChildren")]
[PrimaryKey(nameof(this.ClassroomCode), nameof(this.ChildId))]
public record ClassroomChild
{
    public ClassroomChild()
    {
    }
    
    public ClassroomChild(string classroomCode, string childId)
    {
        ClassroomCode = classroomCode;
        ChildId = childId;
    }

    public string ClassroomCode { get; init; } = null!;
    public string ChildId { get; init; } = null!;

    // Navigation
    
    [ForeignKey(nameof(ClassroomCode))]
    public Classroom Classroom { get; set; } = null!;
    
    [ForeignKey(nameof(ChildId))]
    public Child Child { get; set; } = null!;
    
    public ICollection<ClassroomAnnouncement> Announcements { get; set; } = null!;
}