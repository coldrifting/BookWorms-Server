using BookwormsServer.Models.Entities;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Models.Data;

public record BookDetailsDTO(
    string Title,
    List<string> Authors,
    double Rating,
    string Difficulty,
    string Image,
    string Description,
    List<string> Subjects,
    string Isbn10,
    string Isbn13,
    string Publisher,
    string PublishDate,
    int PageCount,
    List<ReviewDTO> Reviews)
{
    public static BookDetailsDTO From(Book book, JObject bookDetails)
    {
        string title = book.Title;
        List<string> authors = book.Authors;
        double rating = book.StarRating ?? -1;
        string difficulty = book.Level ?? "";
        string image = bookDetails["image"]?.ToObject<string>() ?? "";
        string description = bookDetails["description"]?.ToObject<string>() ?? "";
        List<string> subjects = bookDetails["subjects"]?.ToObject<List<string>>() ?? [];
        string isbn10 = bookDetails["industryIdentifiers"]?[1]?["identifier"]?.ToObject<string>() ?? "";
        string isbn13 = bookDetails["industryIdentifiers"]?[0]?["identifier"]?.ToObject<string>() ?? "";
        string publisher = bookDetails["publisher"]?.ToObject<string>() ?? "";
        string publishedDate = bookDetails["publishedDate"]?.ToObject<string>() ?? "";
        int pageCount = bookDetails["pageCount"]!.ToObject<int>();
        List<ReviewDTO> reviews = book.Reviews.Select(ReviewDTO.From).ToList();
        
        return new BookDetailsDTO(title, authors, rating, difficulty, image, description, subjects, isbn10, isbn13,
            publisher, publishedDate, pageCount, reviews); 
    }
}