using BookwormsServer.Models.Entities;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Models.Data;

public record BookDto(string Title, List<string> Authors, double Rating, double Difficulty, string Image)
{
    public static BookDto From(Book book, JObject bookDetails)
    {
        string title = book.Title;
        List<string> authors = book.Authors;
        double rating = book.StarRating ?? -1.0;
        double difficulty = 0.0;
        string image = bookDetails["image"]!.ToObject<string>() ?? "";
        
        return new BookDto(title, authors, rating, difficulty, image);
    }
}