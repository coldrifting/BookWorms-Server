namespace BookwormsServer.Models.Data;

public record BookResponse(
    string BookId,
    string Title,
    List<string> Authors,
    double? Rating,
    int? Level);

public record BookResponseExtended(
    string BookId,
    string Title,
    List<string> Authors,
    double? Rating,
    int? Level,

    string Description,
    List<string> Subjects,
    string? Isbn10,
    string? Isbn13,
    int PublishYear,
    int? PageCount,
    List<ReviewResponse> Reviews);

public record BookResponsePreview(
    string BookId, 
    string Title, 
    List<string> Authors,
    double? Rating,
    int? Level);