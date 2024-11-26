using System.Text.Json.Nodes;
using BookwormsServer.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BookwormsServer.Services;

public class GoogleBooksApiService(IHttpClientFactory factory, IMemoryCache cache) : IBookApiService
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task<JsonObject> GetData(string bookId)
    {
        string endpoint =
            $"https://www.googleapis.com/books/v1/volumes/{bookId}";
        
        string key = "&key=AIzaSyCMAFln3TxoTl0R9P-2IBPBer36d0HV7Ek";
        
        if (cache.TryGetValue(endpoint, out JsonObject? cachedResponse))
        {
            return cachedResponse ?? [];
        }
        
        var response = await _client.GetAsync(endpoint);

        try
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<JsonObject>();
            
            cache.Set(endpoint, content["volumeInfo"], TimeSpan.FromMinutes(60));
            return content["volumeInfo"] as JsonObject;

        }
        catch (HttpRequestException ex)
        {
            return [];
        }
    }

    public async Task<byte[]> GetImage(string bookId)
    {
        string endpoint =
            $"https://books.google.com/books/publisher/content?id={bookId}&printsec=frontcover&img=1&zoom=1";

        string key = "&key=AIzaSyCMAFln3TxoTl0R9P-2IBPBer36d0HV7Ek";
        
        if (cache.TryGetValue(endpoint, out byte[]? cachedResponse))
        {
            return cachedResponse ?? [];
        }

        var response = await _client.GetAsync(endpoint);

        try
        {
            response.EnsureSuccessStatusCode();
            byte[] inputBytes = await response.Content.ReadAsByteArrayAsync();

            cache.Set(endpoint, inputBytes, TimeSpan.FromMinutes(60));
            return inputBytes;

        }
        catch (HttpRequestException ex)
        {
            // TODO
            return [];
        }
    }
}