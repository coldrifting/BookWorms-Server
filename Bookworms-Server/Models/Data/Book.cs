using System.Text.Json.Nodes;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record BookDto(string BookId, string Title, List<string> Authors, double Rating, string Difficulty)
{
    public static BookDto From(Book book, JsonObject bookDetails)
    {
        string bookId = book.BookId;
        string title = book.Title;
        List<string> authors = book.Authors;
        double rating = book.StarRating ?? -1.0;
        string difficulty = book.Level ?? "";
        
        return new BookDto(bookId, title, authors, rating, difficulty);
    }
}