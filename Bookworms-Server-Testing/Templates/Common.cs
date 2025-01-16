using System.Net;
using System.Net.Http.Json;
using BookwormsServer.Models.Data;

namespace BookwormsServerTesting.Templates;

public static class Common
{
    private const string LoginEndpoint = "/user/login";

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
        if (client.DefaultRequestHeaders.Authorization is null)
        {
            string token = await GetUserToken(client, username, password);
            client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        }
    }

    public static async Task<HttpResponseMessage> GetAsyncAsUser(this HttpClient client, string url, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.GetAsync(url);
    }

    public static async Task<HttpResponseMessage> PutAsJsonAsyncAsUser<TValue>(this HttpClient client, string url, TValue obj, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.PutAsJsonAsync(url, obj);
    }

    public static async Task<HttpResponseMessage> PostAsJsonAsyncAsUser<TValue>(this HttpClient client, string url, TValue obj, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.PostAsJsonAsync(url, obj);
    }

    public static async Task<HttpResponseMessage> DeleteAsyncAsUser(this HttpClient client, string url, string username, string? password = null)
    {
        await VerifyToken(client, username, password ?? username);
        return await client.DeleteAsync(url);
    }
}