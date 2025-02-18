namespace BookwormsServer.Models.Data;

public record ReviewResponse(
    string ReviewerFirstName,
    string ReviewerLastName,
    string ReviewerRole,
    int ReviewerIcon,
    DateTime ReviewDate,
    double StarRating,
    string ReviewText);

public record ReviewEditRequest(
    double StarRating, 
    string? ReviewText = null);
