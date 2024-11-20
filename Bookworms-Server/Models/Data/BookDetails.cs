using BookwormsServer.Models.Entities;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Models.Data;

public record BookDetailsDTO(string Title, string Author, List<string> Subjects, int PageCount, string Isbn10,
    string Isbn13, string Description, string Publisher, string PublishDate, List<ReviewDTO> Reviews)
{
    public static BookDetailsDTO From(Book book, JObject bookDetails)
    {
        string title = book.Title;
        string author = book.Author;
        List<string> subjects = bookDetails["subjects"]?.ToObject<List<string>>() ?? [];
        int pageCount = bookDetails["pageCount"]!.ToObject<int>();
        string isbn10 = bookDetails["industryIdentifiers"]?[1]?["identifier"]?.ToObject<string>() ?? "";
        string isbn13 = bookDetails["industryIdentifiers"]?[0]?["identifier"]?.ToObject<string>() ?? "";
        string description = bookDetails["description"]?.ToObject<string>() ?? "";
        string publisher = bookDetails["publisher"]?.ToObject<string>() ?? "";
        string publishDate = bookDetails["publishedDate"]?.ToObject<string>() ?? "";
        foreach (Review review in book.Reviews)
        {
            Console.WriteLine($"{review.Reviewer}: {review.ReviewText}");
        }
        List<ReviewDTO> reviews = (book.Reviews ?? []).Select(ReviewDTO.From).ToList();
        
        return new BookDetailsDTO(
            title, author, subjects, pageCount, isbn10, isbn13, description, publisher, publishDate, reviews); 
    }
}