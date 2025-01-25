using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookwormsServer.Models.Data;

namespace BookwormsServerTesting.Templates;

public static class Common
{
    private const string LoginEndpoint = "/user/login";

    // HTTP helpers
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static async Task<TValue?> ReadJsonAsync<TValue>(this HttpContent content)
    {
        return await content.ReadFromJsonAsync<TValue>(JsonSerializerOptions);
    }
    
    private static async Task<string> GetUserToken(HttpClient client, string username, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(LoginEndpoint, new UserLoginDTO(username, password));

        if (response.StatusCode == HttpStatusCode.OK)
        {
            UserLoginSuccessDTO? dto = await response.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();
            if (dto is not null)
            {
                return dto.Token;
            }
        }
        
        throw new IOException("Unable to grab token for specified user");
    }

    private static async Task VerifyToken(HttpClient client, string username, string password)
    {
        // Decode Auth Header
        if (client.DefaultRequestHeaders.Authorization is { } auth)
        {
            var curToken = auth.Parameter;
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(curToken);
            var loggedInUser = jwtSecurityToken.Claims.First(claim => claim.Type == "sub").Value;

            if (loggedInUser == username)
            {
                return;
            }
        }
        
        string token = await GetUserToken(client, username, password);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }
    
    public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, string url)
    {
        return await client.PostAsync(url, new StringContent(""));
    }
    
    public static async Task<HttpResponseMessage> PostAsyncAsUser(this HttpClient client, string url, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.PostAsync(url, new StringContent(""));
    }
    
    public static async Task<HttpResponseMessage> PutAsync(this HttpClient client, string url)
    {
        return await client.PutAsync(url, new StringContent(""));
    }
    
    public static async Task<HttpResponseMessage> PutAsyncAsUser(this HttpClient client, string url, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.PutAsync(url, new StringContent(""));
    }
    
    public static async Task<HttpResponseMessage> GetAsyncAsUser(this HttpClient client, string url, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.GetAsync(url);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsyncAsUser<TValue>(this HttpClient client, string url, TValue obj, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.PostAsJsonAsync(url, obj);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsyncAsUser<TValue>(this HttpClient client, string url, TValue obj, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.PutAsJsonAsync(url, obj);
    }

    public static async Task<HttpResponseMessage> DeleteAsyncAsUser(this HttpClient client, string url, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.DeleteAsync(url);
    }
    
    // Test helpers
    public static async Task CheckForError(Func<Task<HttpResponseMessage>> func, HttpStatusCode statusCode, ErrorDTO errorType)
    {
        HttpResponseMessage response = await func.Invoke();
        Assert.Equal(statusCode, response.StatusCode);
        
        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);
        Assert.Equal(errorType, content);
    }
}