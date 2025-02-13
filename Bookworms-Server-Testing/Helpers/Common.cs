using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookwormsServer.Models.Data;

namespace BookwormsServerTesting.Helpers;

public static class Common
{
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
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.User.Login, new UserLoginDTO(username, password));

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

    private static async Task VerifyOrDeleteToken(HttpClient client, string? username, string? password)
    {
        if (username is not null)
        {
            await VerifyToken(client, username, password ?? username);
        }
        else
        {
            client.DefaultRequestHeaders.Authorization = null;
        }
    }
    
    public static async Task<HttpResponseMessage> GetAsync(this HttpClient client, string url, string? username = null, string? password = null)
    {
        await VerifyOrDeleteToken(client, username, password);
        return await client.GetAsync(url);
    }
    
    public static async Task<HttpResponseMessage> PostAsync(this HttpClient client, string url, string? username = null, string? password = null)
    {
        await VerifyOrDeleteToken(client, username, password);
        return await client.PostAsync(url, new StringContent(""));
    }

    public static async Task<HttpResponseMessage> PostPayloadAsync<TValue>(this HttpClient client, string url, TValue obj, string? username = null, string? password = null)
    {
        await VerifyOrDeleteToken(client, username, password);
        return await client.PostAsJsonAsync(url, obj);
    }
    
    public static async Task<HttpResponseMessage> PutAsync(this HttpClient client, string url, string? username = null, string? password = null)
    {
        await VerifyOrDeleteToken(client, username, password);
        return await client.PutAsync(url, new StringContent(""));
    }

    public static async Task<HttpResponseMessage> PutPayloadAsync<TValue>(this HttpClient client, string url, TValue obj, string? username = null, string? password = null)
    {
        await VerifyOrDeleteToken(client, username, password);
        return await client.PutAsJsonAsync(url, obj);
    }

    public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient client, string url, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.DeleteAsync(url);
    }
    
    // Test helpers
    public static async Task CheckForError(Func<Task<HttpResponseMessage>> requestFunc, HttpStatusCode statusCode, ErrorDTO errorType)
    {
        HttpResponseMessage response = await requestFunc.Invoke();
        Assert.Equal(statusCode, response.StatusCode);
        
        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);
        Assert.Equal(errorType, content);
    }
    
    public static async Task CheckResponse(Func<Task<HttpResponseMessage>> requestFunc, HttpStatusCode statusCode)
    {
        HttpResponseMessage response = await requestFunc.Invoke();
        Assert.Equal(statusCode, response.StatusCode);
    }
    
    public static async Task CheckResponse<T>(Func<Task<HttpResponseMessage>> requestFunc, HttpStatusCode statusCode, Action<T> check)
    {
        HttpResponseMessage response = await requestFunc.Invoke();
        Assert.Equal(statusCode, response.StatusCode);

        T? content = await response.Content.ReadJsonAsync<T>();
        Assert.NotNull(content);
        check.Invoke(content);
    }
    
    public static async Task CheckResponse<TContent>(Func<Task<HttpResponseMessage>> requestFunc, HttpStatusCode statusCode, Action<TContent,HttpResponseHeaders> check)
    {
        HttpResponseMessage response = await requestFunc.Invoke();
        Assert.Equal(statusCode, response.StatusCode);

        TContent? content = await response.Content.ReadJsonAsync<TContent>();
        Assert.NotNull(content);
        check.Invoke(content, response.Headers);
    }
    
    public static async Task<TOutput> CheckResponse<TContent,TOutput>(Func<Task<HttpResponseMessage>> requestFunc, HttpStatusCode statusCode, Func<TContent,HttpResponseHeaders,TOutput> check)
    {
        HttpResponseMessage response = await requestFunc.Invoke();
        Assert.Equal(statusCode, response.StatusCode);

        TContent? content = await response.Content.ReadJsonAsync<TContent>();
        Assert.NotNull(content);
        return check.Invoke(content, response.Headers);
    }
    
    public static async Task<TOutput> CheckResponse<TContent,TOutput>(Func<Task<HttpResponseMessage>> requestFunc, HttpStatusCode statusCode, Func<TContent,TOutput> check)
    {
        HttpResponseMessage response = await requestFunc.Invoke();
        Assert.Equal(statusCode, response.StatusCode);

        TContent? content = await response.Content.ReadJsonAsync<TContent>();
        Assert.NotNull(content);
        return check.Invoke(content);
    }

    public static string? GetChildLocation(this HttpResponseHeaders headers)
    {
        return headers.Location?.ToString().Replace("/children/", "");
    }
}