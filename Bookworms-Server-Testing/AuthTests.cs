using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class AuthTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    [Fact]
    public async Task CreateUser()
    {
        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync("/user/register",
            new UserRegisterDTO("user42", "improbable", "42", "user@42.com"));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
    }

    [Fact]
    public async Task Login()
    {
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("teacher1", "teacher1"));
        
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        UserLoginSuccessDTO? token = await loginResponse.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);
    }
    
    [Fact]
    public async Task CreateUserAndLogin()
    {
        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync("/user/register",
            new UserRegisterDTO("user42", "improbable", "42", "user@42.com"));

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("user42", "improbable"));
        
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        UserLoginSuccessDTO? token = await loginResponse.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);
    }
    
    [Fact]
    public async Task AdminLoginShouldReturnToken()
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("admin", "admin"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserLoginSuccessDTO? token = await response.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);
    }

    // TODO - Figure out why testing doesn't work for authorized routes
    /*
    [Fact]
    public async Task Test()
    {
        var response = await Client.GetAsync("/user/users");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ShowUsersAsAdminShouldOk()
    {
        var prelim = await Client.PostAsJsonAsync("/user/login", new UserLoginDTO("admin", "admin"));

        var t = await prelim.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        //Client.DefaultRequestHeaders.Authorization = new("Bearer", t.Token);
        
        var response = await Client.GetAsync("/user/users");
        
        //var response = await Client.GetAsyncAsUser("/user/users", "admin", "admin");
        
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
        var response = await Client.GetAsyncAsUser("/user/users", "basicUser", "basicUser");
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();

        Assert.NotNull(content);
        Assert.Equal("Forbidden", content.Error);
    }
    
    [Fact]
    public async Task ShowUsersAsUnAuthenticatedShouldUnauthorized()
    {
        var response = await Client.GetAsync("/user/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ErrorDTO>();

        Assert.NotNull(content);
        Assert.Equal("Forbidden", content.Error);
    }
    */
}