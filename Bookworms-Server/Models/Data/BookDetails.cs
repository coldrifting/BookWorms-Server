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
        string isbn10 = GetIsbn(bookDetails, true);
        string isbn13 = GetIsbn(bookDetails, false);
        string publisher = bookDetails["publisher"]?.ToObject<string>() ?? "";
        string publishedDate = bookDetails["publishedDate"]?.ToObject<string>() ?? "";
        int pageCount = bookDetails["pageCount"]!.ToObject<int>();
        List<ReviewDTO> reviews = book.Reviews.Select(ReviewDTO.From).ToList();
        
        return new BookDetailsDTO(title, authors, rating, difficulty, image, description, subjects, isbn10, isbn13,
            publisher, publishedDate, pageCount, reviews); 
    }

    private static string GetIsbn(JObject bookDetails, bool useIsbn10)
    {
        int index = useIsbn10 ? 1 : 0;

        try
        {
            return bookDetails["industryIdentifiers"]?[index]?["identifier"]?.ToObject<string>() ?? "";
        }
        catch
        {
            return "";
        }
    }
}