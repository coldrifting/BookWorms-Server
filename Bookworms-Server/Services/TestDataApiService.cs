using System.Text.Json.Nodes;
using BookwormsServer.Services.Interfaces;

namespace BookwormsServer.Services;

public class TestDataApiService : IBookApiService
{
    private readonly JsonObject? _books = JsonNode.Parse(File.ReadAllText("TestData/ApiBooks.json")) as JsonObject;

    public Task<JsonObject> GetData(string bookId)
    {
        return Task.FromResult(_books?[bookId] as JsonObject ?? new JsonObject());
    }
    
    public Task<byte[]> GetImage(string bookId)
    {
        throw new NotImplementedException();
    }
}