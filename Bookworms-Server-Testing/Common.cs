using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BookwormsServer.Models.Data;

namespace BookwormsServerTesting;

public static class Common
{
    public static string LoginEndpoint = "/account/login";
    
    public static async Task<string> GetUserToken(HttpClient client, string username, string password)
    {
        var response = await client.PostAsJsonAsync(LoginEndpoint, new UserLoginDTO(username, password));

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var dto = await response.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();
            if (dto is not null)
            {
                return dto.Token;
            }
        }

        throw new IOException("Unable to grab token for user");
    }

    public static async Task<HttpResponseMessage> GetAsyncAsUser(this HttpClient client, string url, string username, string password)
    {
        string token = await GetUserToken(client, username, password);

        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        HttpResponseMessage response = await client.GetAsync(url);

        client.DefaultRequestHeaders.Authorization = null;

        return response;
    }
}