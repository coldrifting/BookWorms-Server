using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record BookDTO(string BookId, string Title, List<string> Authors, double Rating, int? Level)
{
    public static BookDTO From(Book book)
    {
        string bookId = book.BookId;
        string title = book.Title;
        List<string> authors = book.Authors;
        double rating = book.StarRating ?? -1.0;
        int? level = book.Level;
        
        return new(bookId, title, authors, rating, level);
    }
}