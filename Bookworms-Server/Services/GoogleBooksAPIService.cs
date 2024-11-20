using BookwormsServer.Services.Interfaces;
using Newtonsoft.Json.Linq;

namespace BookwormsServer.Services;

public class GoogleBooksApiService : IBookApiService
{
    private JObject _books = JObject.Parse(File.ReadAllText("TestData/ApiBooks.json"));

    public string GetData(string bookId)
    {
        return this._books[bookId]!.ToString();
    }
}