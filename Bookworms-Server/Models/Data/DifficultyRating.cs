namespace BookwormsServer.Models.Data;

public record UpdatedLevelResponse(
    string EntityTypeName,
    string EntityId,
    int? OldLevel,
    int? NewLevel);

public record DifficultyRatingAddRequest(
    string ChildId,
    int Rating);