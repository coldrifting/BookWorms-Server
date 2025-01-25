using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using static BookwormsServerTesting.Templates.Common;

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

    [Theory]
    [InlineData("userDoesNotExist", "wrongPassword")]
    [InlineData("userDoesNotExist", "teacher1")]
    [InlineData("wrongUsername", "wrongPassword")]
    public async Task Test_LoginBadUsernameAndOrPassword(string username, string password)
    {
        await CheckForError(() => Client.PostAsJsonAsync("/user/login", 
                new UserLoginDTO(username, password)), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.LoginFailure);
    }
    
    [Fact]
    public async Task Test_CreateNewUserAndLogin()
    {
        HttpResponseMessage registerResponse = await Client.PostAsJsonAsync("/user/register",
            new UserRegisterDTO("user23", "improbable", "23", "19", true));
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        
        UserLoginSuccessDTO? token1 = await registerResponse.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();
        Assert.NotNull(token1);
        Assert.NotEmpty(token1.Token);
        
        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync("/user/login", 
            new UserLoginDTO("user23", "improbable"));
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        
        UserLoginSuccessDTO? token2 = await loginResponse.Content.ReadFromJsonAsync<UserLoginSuccessDTO>();
        Assert.NotNull(token2);
        Assert.NotEmpty(token2.Token);
    }

    [Theory]
    [InlineData("teacher0", "somePassword")]
    public async Task Test_CreateUserUsernameAlreadyExists(string username, string password)
    {
        await CheckForError(() => Client.PostAsJsonAsync("/user/register", 
                new UserRegisterDTO(username, password, username, username, false)), 
            HttpStatusCode.UnprocessableContent, 
            ErrorDTO.UsernameAlreadyExists);
    }
    
    [Theory]
    [InlineData("admin", "admin")]
    public async Task Test_LoginExistingAccountShouldReturnToken(string username, string password)
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
        
        var users = await response.Content.ReadFromJsonAsync<List<UserDetailsDTO>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        Assert.Equal("admin", users[0].Username);
    }
    
    [Theory]
    [InlineData("parent0")]
    [InlineData("teacher3")]
    public async Task Test_ShowUsersAsRegularUserShouldForbid(string username)
    {
        await CheckForError(() => Client.GetAsyncAsUser("/user/all", username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotAdmin);
    }
    
    [Fact]
    public async Task Test_ShowUsersAsUnAuthenticatedShouldUnauthorized()
    {
        await CheckForError(() => Client.GetAsync("/user/all"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Fact]
    public async Task Test_UserDetails_NotLoggedInShouldError()
    {
        await CheckForError(() => Client.GetAsync("/user/details"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("teacher1", "Emma", "Johnson", "Teacher", "Icon2")]
    [InlineData("parent1", "Liam", "Smith", "Parent", "Icon1")]
    [InlineData("admin", "Admin", "Admin", "Admin", "Icon3")]
    public async Task Test_UserDetails_Basic(string username, string firstName, string lastName, string role, string icon)
    {
        HttpResponseMessage response = await Client.GetAsyncAsUser("/user/details", username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        UserDetailsDTO? content = await response.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content);
        Assert.Equal(username, content.Username);
        Assert.Equal(firstName, content.FirstName);
        Assert.Equal(lastName, content.LastName);
        Assert.Equal(role, content.Role);
        Assert.Equal(icon, content.Icon);
    }
    
    [Fact]
    public async Task Test_UserEdit_NotLoggedInShouldError()
    {
        await CheckForError(() => Client.PostAsync("/user/edit"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("teacher1", "New")]
    [InlineData("parent1", "First")]
    [InlineData("admin", "Name")]
    public async Task Test_UserEdit_FirstNameOnly(string username, string newFirstName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser("/user/edit", new UserDetailsEditDTO(FirstName: newFirstName), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        UserDetailsDTO? content = await response.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content);
        Assert.Equal(username, content.Username);
        Assert.Equal(newFirstName, content.FirstName);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser("/user/details", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        UserDetailsDTO? content2 = await response2.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content2);
        Assert.Equal(username, content2.Username);
        Assert.Equal(newFirstName, content2.FirstName);
    }
    
    [Theory]
    [InlineData("teacher1", "New")]
    [InlineData("parent1", "First")]
    [InlineData("admin", "Name")]
    public async Task Test_UserEdit_LastNameOnly(string username, string newLastName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser("/user/edit", new UserDetailsEditDTO(LastName: newLastName), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        UserDetailsDTO? content = await response.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content);
        Assert.Equal(username, content.Username);
        Assert.Equal(newLastName, content.LastName);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser("/user/details", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        UserDetailsDTO? content2 = await response2.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content2);
        Assert.Equal(username, content2.Username);
        Assert.Equal(newLastName, content2.LastName);
    }
    
    [Theory]
    [InlineData("teacher1", "Icon1")]
    [InlineData("parent1", "Icon3")]
    [InlineData("admin", "Icon3")]
    public async Task Test_UserEdit_IconOnly(string username, string newIcon)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser("/user/edit", new UserDetailsEditDTO(Icon: newIcon), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        UserDetailsDTO? content = await response.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content);
        Assert.Equal(username, content.Username);
        Assert.Equal(newIcon, content.Icon);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser("/user/details", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        UserDetailsDTO? content2 = await response2.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content2);
        Assert.Equal(username, content2.Username);
        Assert.Equal(newIcon, content2.Icon);
    }
    
    [Theory]
    [InlineData("teacher1", "IconX")]
    [InlineData("parent1", "Blah")]
    [InlineData("admin", "Iconzzzz")]
    public async Task Test_UserEdit_IconOnlyInvalid(string username, string newIcon)
    {
        await CheckForError(() => Client.PostAsJsonAsyncAsUser("/user/edit",
                new UserDetailsEditDTO(Icon: newIcon), username), 
            HttpStatusCode.UnprocessableEntity, 
            ErrorDTO.InvalidIconIndex);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser("/user/details", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        UserDetailsDTO? content2 = await response2.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content2);
        Assert.Equal(username, content2.Username);
        Assert.NotEqual(newIcon, content2.Icon);
    }
    
    [Theory]
    [InlineData("teacher1", "NewPassWord")]
    [InlineData("parent1", "NewPassWord")]
    [InlineData("admin", "NewPassWord")]
    public async Task Test_UserEdit_PasswordOnly(string username, string newPassword)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser("/user/edit", new UserDetailsEditDTO(Password: newPassword), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        UserDetailsDTO? content = await response.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content);
        Assert.Equal(username, content.Username);

        // Clear token to force re-login with new password
        Client.DefaultRequestHeaders.Authorization = null;
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser("/user/details", username, newPassword);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        UserDetailsDTO? content2 = await response2.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content2);
        Assert.Equal(username, content2.Username);
    }
    
    [Theory]
    [InlineData("teacher1", "NewFirstName", "NewLastName", "Icon2", "NewPassWord")]
    [InlineData("parent1", "NewFirstName", "NewLastName", "Icon2", "NewPassWord")]
    [InlineData("admin", "NewFirstName", "NewLastName", "Icon2", "NewPassWord")]
    public async Task Test_UserEdit_AllValid(string username, string newFirstName, string newLastName, string newIcon, string newPassword)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser("/user/edit", new UserDetailsEditDTO(newFirstName, newLastName, newIcon, newPassword), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        UserDetailsDTO? content = await response.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content);
        Assert.Equal(username, content.Username);
        Assert.Equal(newFirstName, content.FirstName);
        Assert.Equal(newLastName, content.LastName);
        Assert.Equal(newIcon, content.Icon);
        
        // Clear token to force re-login with new password
        Client.DefaultRequestHeaders.Authorization = null;
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser("/user/details", username, newPassword);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        UserDetailsDTO? content2 = await response2.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content2);
        Assert.Equal(username, content2.Username);
        Assert.Equal(newFirstName, content2.FirstName);
        Assert.Equal(newLastName, content2.LastName);
        Assert.Equal(newIcon, content2.Icon);
    }
    
    [Theory]
    [InlineData("teacher1", "NewFirstName", "NewLastName", "BadIconIndex", "NewPassWord")]
    [InlineData("parent1", "NewFirstName", "NewLastName", "BadIconIndex", "NewPassWord")]
    [InlineData("admin", "NewFirstName", "NewLastName", "BadIconIndex", "NewPassWord")]
    public async Task Test_UserEdit_AllValidExceptIcon(string username, string newFirstName, string newLastName, string newIcon, string newPassword)
    {
        await CheckForError(() => Client.PostAsJsonAsyncAsUser("/user/edit",
            new UserDetailsEditDTO(newFirstName, newLastName, newIcon, newPassword), username), 
            HttpStatusCode.UnprocessableEntity, 
            ErrorDTO.InvalidIconIndex);
        
        // Clear token to force re-login with new password
        Client.DefaultRequestHeaders.Authorization = null;
        
        // Ensure that password did not change
        await CheckForError(() => Client.PostAsJsonAsyncAsUser("/user/login",
            new UserLoginDTO(username, newPassword), username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.LoginFailure);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser("/user/details", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        UserDetailsDTO? content3 = await response3.Content.ReadFromJsonAsync<UserDetailsDTO>();
        Assert.NotNull(content3);
        Assert.Equal(username, content3.Username);
        Assert.NotEqual(newFirstName, content3.FirstName);
        Assert.NotEqual(newLastName, content3.LastName);
        Assert.NotEqual(newIcon, content3.Icon);
    }
}