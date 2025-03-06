using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookwormsServer.Models.Data;
using BookwormsServer.Utils;

namespace BookwormsServer.Models.Entities;

public class ClassroomAnnouncement(string classCode, string title, string body, DateTime time)
{
    [Key]
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

    public ClassroomAnnouncementResponse ToResponse()
    {
        return new(AnnouncementId, Title, Body, Time);
    }
}