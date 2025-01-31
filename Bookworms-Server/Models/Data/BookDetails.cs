using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record BookDetailsDTO(
    string Description,
    List<string> Subjects,
    string Isbn10,
    string Isbn13,
    int PublishYear,
    int? PageCount,
    List<ReviewDTO> Reviews)
{
    public static BookDetailsDTO From(Book book)
    {
        string description = book.Description;
        List<string> subjects = book.Subjects;
        string isbn10 = book.Isbn10;
        string isbn13 = book.Isbn13;
        int publishYear = book.PublishYear;
        int? pageCount = book.PageCount;
        List<ReviewDTO> reviews = book.Reviews.Select(ReviewDTO.From).ToList();
        
        return new(description, subjects, isbn10, isbn13, publishYear, pageCount, reviews); 
    }
}