using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;
using BookwormsServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer.Models.Entities;

[PrimaryKey(nameof(this.AnnouncementId), nameof(this.ClassCode))]
public class ClassroomAnnouncement(string classCode, string title, string body, DateTime time)
{
    [StringLength(14)]
    [Column(TypeName="char")]
    public string AnnouncementId { get; set; } = Snowflake.Generate();
    
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Classroom code must be exactly {1} characters long.")]
    public string ClassCode { get; set; } = classCode;

    [StringLength(256, ErrorMessage = "Announcement Title cannot be longer than {0} characters.")]
    public string Title { get; set; } = title;

    [StringLength(1024, ErrorMessage = "Announcement Body cannot be longer than {0} characters.")]
    public string Body { get; set; } = body;

    public DateTime Time { get; set; } = time;

    // Navigation
    [ForeignKey(nameof(ClassCode))] 
    public Classroom Classroom { get; set; } = null!;
    
    public ICollection<ClassroomChild> ClassroomChildren { get; set; } = null!;

    public ClassroomAnnouncementResponse ToTeacherResponse()
    {
        return new(AnnouncementId, Title, Body, Time);
    }

    public ClassroomAnnouncementResponse ToChildResponse()
    {
        // TODO
        return new(AnnouncementId, Title, Body, Time, false);
    }
}

[PrimaryKey(nameof(AnnouncementId), nameof(ChildId))]
public class ClassroomAnnouncementsRead(string announcementId, string classCode, string childId)
{
    [StringLength(14), Column(TypeName="char")]
    public string AnnouncementId { get; set; } = announcementId;
    
    [StringLength(6), Column(TypeName = "char")]
    public string ClassCode { get; set; } = classCode;
    
    [StringLength(14), Column(TypeName = "char")]
    public string ChildId { get; set; } = childId;
    
    // Navigation
    [ForeignKey(nameof(AnnouncementId) + "," + nameof(ClassCode))]
    public ClassroomAnnouncement Announcement { get; set; } = null!;
    
    [ForeignKey(nameof(ClassCode) + "," + nameof(ChildId))]
    public ClassroomChild ClassroomChild { get; set; } = null!;
}