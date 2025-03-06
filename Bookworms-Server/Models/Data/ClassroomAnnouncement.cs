namespace BookwormsServer.Models.Data;

public record ClassroomAnnouncementAddRequest(
    string Title,
    string Body
    );

public record ClassroomAnnouncementEditRequest(
    string? Title,
    string? Body
    );

public record ClassroomAnnouncementResponse(
    string AnnoucementId,
    string Title,
    string Body,
    DateTime Time
    );