using System.Text.Json;
using System.Text.Json.Nodes;
using BookwormsServer.Models.Entities;

namespace BookwormsServer.Models.Data;

public record BookDetailsDTO(
    string Description,
    List<string> Subjects,
    string Isbn10,
    string Isbn13,
    string Publisher,
    string PublishDate,
    int PageCount,
    List<ReviewDTO> Reviews)
{
    public static BookDetailsDTO From(Book book, JsonObject bookDetails)
    {
        string title = book.Title;
        List<string> authors = book.Authors;
        double rating = book.StarRating ?? -1;
        string difficulty = book.Level ?? "";
        string description = bookDetails["description"]?.Deserialize<string>() ?? "";
        List<string> subjects = bookDetails["subjects"]?.Deserialize<List<string>>() ?? [];
        string isbn10 = GetIsbn(bookDetails, true);
        string isbn13 = GetIsbn(bookDetails, false);
        string publisher = bookDetails["publisher"]?.Deserialize<string>() ?? "";
        string publishedDate = bookDetails["publishedDate"]?.Deserialize<string>() ?? "";
        int pageCount = bookDetails["pageCount"]?.Deserialize<int>() ?? -1;
        List<ReviewDTO> reviews = book.Reviews.Select(ReviewDTO.From).ToList();
        
        return new BookDetailsDTO(description, subjects, isbn10, isbn13,
            publisher, publishedDate, pageCount, reviews); 
    }

    private static string GetIsbn(JsonObject bookDetails, bool useIsbn10)
    {
        int index = useIsbn10 ? 0 : 1;

        try
        {
            return bookDetails["industryIdentifiers"]?[index]?["identifier"]?.Deserialize<string>() ?? "";
        }
        catch
        {
            return "";
        }
    }
}