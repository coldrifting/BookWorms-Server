using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record BookDetailsDTO(
    string BookId,
    string Title,
    List<string> Authors,
    double? Rating)
{
    public static BookDetailsDTO From(Book book)
    {
        return new(
            book.BookId, 
            book.Title, 
            book.Authors, 
            book.StarRating);
    }
}

public record BookDetailsExtendedDTO(
    string BookId,
    string Title,
    List<string> Authors,
    double? Rating,
    string Description,
    List<string> Subjects,
    string? Isbn10,
    string? Isbn13,
    int PublishYear,
    int? PageCount,
    List<ReviewDTO> Reviews)
{
    public static BookDetailsExtendedDTO From(Book book)
    {
        return new(
            book.BookId, 
            book.Title, 
            book.Authors, 
            book.StarRating,
            book.Description,
            book.Subjects,
            book.Isbn10 == "" ? null : book.Isbn10,
            book.Isbn13 == "" ? null : book.Isbn13,
            book.PublishYear,
            book.PageCount,
            book.Reviews.Select(ReviewDTO.From).ToList()
            );
    }
}