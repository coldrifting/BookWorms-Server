using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class UserLoginTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    [Theory]
    [InlineData("testParent", "testParentName", true)]
    [InlineData("testTeacher", "testName", false)]
    public async Task Test_CreateUserBasic(string username, string name, bool isParent)
    {
        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync("/user/register",
            new UserRegisterDTO(username, username, name, name, isParent));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
    }

    [Fact]
    public async Task Test_LoginBasic()
    {
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("teacher1", "teacher1"));
        
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        UserLoginSuccessDTO? token = await loginResponse.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);
    }

    [Fact]
    public async Task Test_LoginBadUsername()
    {
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("userDoesNotExist", "improbable"));
        
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

        var content = await loginResponse.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);

        Assert.Equal(ErrorDTO.LoginFailure, content);
    }

    [Fact]
    public async Task Test_LoginBadPassword()
    {
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("teacher0", "wrongPassword"));
        
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

        var content = await loginResponse.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);

        Assert.Equal(ErrorDTO.LoginFailure, content);
    }

    [Fact]
    public async Task Test_LoginBadUsernameAndPassword()
    {
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("wrongUsername", "wrongPassword"));
        
        Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

        var content = await loginResponse.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);

        Assert.Equal(ErrorDTO.LoginFailure, content);
    }
    
    [Fact]
    public async Task Test_CreateNewUserAndLogin()
    {
        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync("/user/register",
            new UserRegisterDTO("user23", "improbable", "23", "19", true));

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("user23", "improbable"));
        
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        UserLoginSuccessDTO? token = await loginResponse.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);
    }

    [Fact]
    public async Task Test_CreateUserUsernameAlreadyExists()
    {
        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync("/user/register",
            new UserRegisterDTO("teacher0", "teacher0", "teacher0", "teacher0", false));

        Assert.Equal(HttpStatusCode.Conflict, registerResponse.StatusCode);
        
        ErrorDTO? loginError = await registerResponse.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(loginError);
        Assert.NotEmpty(loginError.Error);
        Assert.NotEmpty(loginError.Description);
        Assert.Equal(ErrorDTO.UsernameAlreadyExists, loginError);
    }
    
    [Theory]
    [InlineData("admin", "admin")]
    public async Task Test_LoginShouldReturnToken(string username, string password)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO(username, password));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        UserLoginSuccessDTO? token = await response.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();

        Assert.NotNull(token);
        Assert.NotEmpty(token.Token);
    }

    [Fact]
    public async Task Test_ShowUsersAsAdminShouldOk()
    {
        HttpResponseMessage response = await Client.GetAsyncAsUser("/user/all", "admin");
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var users = await response.Content.ReadFromJsonAsync<List<UserDTO>>();

        Assert.NotNull(users);
        Assert.NotEmpty(users);
        Assert.Equal("admin", users[0].Username);
        
        Client.DefaultRequestHeaders.Remove("Authorization");
    }
    
    [Fact]
    public async Task Test_ShowUsersAsRegularUserShouldForbid()
    {
        HttpResponseMessage response = await Client.GetAsyncAsUser("/user/all", "parent1");
        
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();

        Assert.NotNull(content);
        Assert.Equal("Forbidden", content.Error);
    }
    
    [Fact]
    public async Task Test_ShowUsersAsUnAuthenticatedShouldUnauthorized()
    {
        HttpResponseMessage response = await Client.GetAsync("/user/all");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();

        Assert.NotNull(content);
        Assert.Equal("Unauthorized", content.Error);
    }
}