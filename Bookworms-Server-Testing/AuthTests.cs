using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class AuthTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    [Fact]
    public async Task AdminLoginShouldReturnToken()
    {
        var response = await Client.PostAsJsonAsync("/account/login", 
            new UserLoginDTO("admin", "admin"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);
    }

    [Fact]
    public async Task Test()
    {
        // TODO - Figure out why this doesn't work for authorized routes
        var response = await Client.GetAsync("/account/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ShowUsersAsAdminShouldOk()
    {
        var prelim = await Client.PostAsJsonAsync("/account/login", new UserLoginDTO("admin", "admin"));

        var t = await prelim.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        //Client.DefaultRequestHeaders.Authorization = new("Bearer", t.Token);
        
        var response = await Client.GetAsync("/account/users");
        
        //var response = await Client.GetAsyncAsUser("/account/users", "admin", "admin");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        User[]? users = await response.Content.ReadFromJsonAsync<User[]>();

        Assert.NotNull(users);
        Assert.Single(users);
        Assert.Equal("admin", users[0].Username);
        
        Client.DefaultRequestHeaders.Remove("Authorization");
    }
    
    [Fact]
    public async Task ShowUsersAsRegularUserShouldForbid()
    {
        var response = await Client.GetAsyncAsUser("/account/users", "basicUser", "basicUser");
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();

        Assert.NotNull(content);
        Assert.Equal("Forbidden", content.Error);
    }
    
    [Fact]
    public async Task ShowUsersAsUnAuthenticatedShouldUnauthorized()
    {
        var response = await Client.GetAsync("/account/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();

        Assert.NotNull(content);
        Assert.Equal("Forbidden", content.Error);
    }
}