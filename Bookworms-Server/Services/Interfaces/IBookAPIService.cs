using System.Text.Json.Nodes;

namespace BookwormsServer.Services.Interfaces;

public interface IBookApiService
{
    public Task<JsonObject> GetData(string bookId);
    public Task<byte[]> GetImage(string bookId);
}