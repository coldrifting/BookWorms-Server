using System.Text.Json.Nodes;
using BookwormsServer.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BookwormsServer.Services;

public class OpenLibraryApiService(IHttpClientFactory factory, IMemoryCache cache) : IBookApiService
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task<JsonObject> GetData(string bookId)
    {
        string endpoint = $"https://www.openlibrary.org/works/{bookId}.json";
        
        if (cache.TryGetValue(endpoint, out JsonObject? cachedResponse))
        {
            return cachedResponse ?? [];
        }
        
        var response = await _client.GetAsync(endpoint);

        try
        {
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<JsonObject>();

            if (content is null) 
                return [];
            
            cache.Set(endpoint, content, TimeSpan.FromMinutes(60));
            return content;
        }
        catch (HttpRequestException)
        {
            return [];
        }
    }

    public async Task<byte[]> GetImage(string imageId)
    {
        string endpoint = $"https://covers.openlibrary.org/b/id/{imageId}-L.jpg";
        
        if (cache.TryGetValue(endpoint, out byte[]? cachedResponse))
        {
            return cachedResponse ?? [];
        }

        var response = await _client.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();
        byte[] inputBytes = await response.Content.ReadAsByteArrayAsync();

        cache.Set(endpoint, inputBytes, TimeSpan.FromMinutes(60));
        return inputBytes;
    }
}