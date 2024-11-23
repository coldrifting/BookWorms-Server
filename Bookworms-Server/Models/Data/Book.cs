using BookwormsServer.Models.Entities;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Models.Data;

public record BookDto(string BookId, string Title, List<string> Authors, double Rating, string Difficulty, string Image)
{
    public static BookDto From(Book book, JObject bookDetails)
    {
        string bookId = book.BookId;
        string title = book.Title;
        List<string> authors = book.Authors;
        double rating = book.StarRating ?? -1.0;
        string difficulty = book.Level ?? "";
        string image = bookDetails["image"]!.ToObject<string>() ?? "";
        
        return new BookDto(bookId, title, authors, rating, difficulty, image);
    }
}