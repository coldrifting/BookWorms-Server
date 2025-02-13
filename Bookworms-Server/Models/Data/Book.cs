using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record BookDTO(string BookId, string Title, List<string> Authors, double? Rating, int? Level)
{
    public static BookDTO From(Book book)
    {
        string bookId = book.BookId;
        string title = book.Title;
        List<string> authors = book.Authors;
        double? rating = book.StarRating;
        int? level = book.Level;
        
        return new(bookId, title, authors, rating, level);
    }
}

public record BookDetailsDTO(
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
    List<ReviewDTO> Reviews)
{
    public static BookDetailsDTO From(Book book)
    {
        return new(
            book.BookId,
            book.Title,
            book.Authors,
            book.StarRating,
            book.Level,
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