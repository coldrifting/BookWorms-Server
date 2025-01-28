using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

public abstract class UserLoginTests
{
    [Collection("Integration Tests")]
    public class UserLoginReadOnlyTests(AppFactory<Program> factory) : BaseTestReadOnlyFixture(factory)
    {
        [Theory]
        [InlineData("admin", "admin")]
        [InlineData("teacher1", "teacher1")]
        [InlineData("parent2", "parent2")]
        public async Task Test_Login_Basics(string username, string password)
        {
            await CheckResponse<UserLoginSuccessDTO>(async () => await Client.PostPayloadAsync(Routes.User.Login,
                    new UserLoginDTO(username, password)),
                HttpStatusCode.OK,
                content =>
                    Assert.NotNull(content.Token)
            );
        }

        [Theory]
        [InlineData("userDoesNotExist", "wrongPassword")]
        [InlineData("userDoesNotExist", "teacher1")]
        [InlineData("wrongUsername", "wrongPassword")]
        public async Task Test_LoginBadUsernameAndOrPassword(string username, string password)
        {
            await CheckForError(() => Client.PostPayloadAsync(Routes.User.Login,
                    new UserLoginDTO(username, password)),
                HttpStatusCode.BadRequest,
                ErrorDTO.LoginFailure);
        }

        [Theory]
        [InlineData("teacher0", "somePassword")]
        public async Task Test_CreateUserUsernameAlreadyExists(string username, string password)
        {
            await CheckForError(() => Client.PostPayloadAsync(Routes.User.Register,
                    new UserRegisterDTO(username, password, username, username, false)),
                HttpStatusCode.UnprocessableContent,
                ErrorDTO.UsernameAlreadyExists);
        }

        [Fact]
        public async Task Test_ShowUsersAsAdminShouldOk()
        {
            await CheckResponse<List<UserDetailsDTO>>(async () => await Client.GetAsync(Routes.User.All, "admin"),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.NotEmpty(content);
                    Assert.Contains(content, u => u is { Username: "admin", Role: "Admin" });
                    Assert.Contains(content, u => u is { Username: "teacher0", Role: "Teacher" });
                    Assert.Contains(content, u => u is { Username: "parent1", Role: "Parent" });
                });
        }

        [Theory]
        [InlineData("parent0")]
        [InlineData("teacher3")]
        public async Task Test_ShowUsersAsRegularUserShouldForbid(string username)
        {
            await CheckForError(() => Client.GetAsync(Routes.User.All, username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotAdmin);
        }

        [Fact]
        public async Task Test_ShowUsersAsUnAuthenticatedShouldUnauthorized()
        {
            await CheckForError(() => Client.GetAsync(Routes.User.All),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Fact]
        public async Task Test_UserDetails_NotLoggedInShouldError()
        {
            await CheckForError(() => Client.GetAsync(Routes.User.Details),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "Emma", "Johnson", "Teacher", 1)]
        [InlineData("parent1", "Liam", "Smith", "Parent", 0)]
        [InlineData("admin", "Admin", "Admin", "Admin", 2)]
        public async Task Test_UserDetails_Basic(string username, string firstName, string lastName, string role,
            int icon)
        {
            await CheckResponse<UserDetailsDTO>(async () => await Client.GetAsync(Routes.User.Details, username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(firstName, content.FirstName);
                    Assert.Equal(lastName, content.LastName);
                    Assert.Equal(role, content.Role);
                    Assert.Equal(icon, content.Icon);
                });
        }

        [Fact]
        public async Task Test_UserEdit_NotLoggedInShouldError()
        {
            await CheckForError(() => Client.PutAsync(Routes.User.Details),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }
    }

    [Collection("Integration Tests")]
    public class UserLoginWriteTests(AppFactory<Program> factory) : BaseTestWriteFixture(factory)
    {
        [Theory]
        [InlineData("testParent", "testParentName", true)]
        [InlineData("testTeacher", "testName", false)]
        public async Task Test_CreateNewUser_Basic(string username, string name, bool isParent)
        {
            await CheckResponse<UserLoginSuccessDTO>(async () => await Client.PostPayloadAsync(Routes.User.Register,
                    new UserRegisterDTO(username, username, name, name, isParent)),
                HttpStatusCode.OK,
                content =>
                    Assert.NotNull(content.Token)
            );
        }

        [Theory]
        [InlineData("testParent", "testParentName", true)]
        [InlineData("testTeacher", "testName", false)]
        public async Task Test_CreateNewUser_ThenLogin(string username, string name, bool isParent)
        {
            await CheckResponse<UserLoginSuccessDTO>(async () => await Client.PostPayloadAsync(Routes.User.Register,
                    new UserRegisterDTO(username, username, name, name, isParent)),
                HttpStatusCode.OK,
                content =>
                    Assert.NotNull(content.Token)
            );

            await CheckResponse<UserLoginSuccessDTO>(async () => await Client.PostPayloadAsync(Routes.User.Login,
                    new UserLoginDTO(username, username)),
                HttpStatusCode.OK,
                content =>
                    Assert.NotNull(content.Token)
            );
        }

        [Theory]
        [InlineData("teacher1", "New")]
        [InlineData("parent1", "First")]
        [InlineData("admin", "Name")]
        public async Task Test_UserEdit_FirstNameOnly(string username, string newFirstName)
        {
            await CheckResponse<UserDetailsDTO>(async () => await Client.PutPayloadAsync(Routes.User.Details,
                    new UserDetailsEditDTO(FirstName: newFirstName), username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newFirstName, content.FirstName);
                });

            await CheckResponse<UserDetailsDTO>(async () => await Client.GetAsync(Routes.User.Details, username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newFirstName, content.FirstName);
                });
        }

        [Theory]
        [InlineData("teacher1", "New")]
        [InlineData("parent1", "First")]
        [InlineData("admin", "Name")]
        public async Task Test_UserEdit_LastNameOnly(string username, string newLastName)
        {
            await CheckResponse<UserDetailsDTO>(async () => await Client.PutPayloadAsync(Routes.User.Details,
                    new UserDetailsEditDTO(LastName: newLastName), username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newLastName, content.LastName);
                });

            await CheckResponse<UserDetailsDTO>(async () => await Client.GetAsync(Routes.User.Details, username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newLastName, content.LastName);
                });
        }

        [Theory]
        [InlineData("teacher1", 0)]
        [InlineData("parent1", 2)]
        [InlineData("admin", 1)]
        public async Task Test_UserEdit_IconOnly(string username, int newIcon)
        {
            await CheckResponse<UserDetailsDTO>(async () => await Client.PutPayloadAsync(Routes.User.Details,
                    new UserDetailsEditDTO(Icon: newIcon), username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newIcon, content.Icon);
                });

            await CheckResponse<UserDetailsDTO>(async () => await Client.GetAsync(Routes.User.Details, username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newIcon, content.Icon);
                });
        }

        [Theory]
        [InlineData("teacher1", "NewPassWord")]
        [InlineData("parent1", "NewPassWord")]
        [InlineData("admin", "NewPassWord")]
        public async Task Test_UserEdit_PasswordOnly(string username, string newPassword)
        {
            await CheckResponse<UserDetailsDTO>(async () => await Client.PutPayloadAsync(Routes.User.Details,
                    new UserDetailsEditDTO(Password: newPassword), username),
                HttpStatusCode.OK,
                content => { Assert.Equal(username, content.Username); });

            // Clear token to force re-login with new password
            Client.DefaultRequestHeaders.Authorization = null;

            await CheckResponse<UserDetailsDTO>(
                async () => await Client.GetAsync(Routes.User.Details, username, newPassword),
                HttpStatusCode.OK,
                content => { Assert.Equal(username, content.Username); });
        }

        [Theory]
        [InlineData("teacher1", "NewFirstName", "NewLastName", 3, "NewPassWord")]
        [InlineData("parent1", "NewFirstName", "NewLastName", 3, "NewPassWord")]
        [InlineData("admin", "NewFirstName", "NewLastName", 3, "NewPassWord")]
        public async Task Test_UserEdit_AllValid(string username, string newFirstName, string newLastName,
            int newIcon, string newPassword)
        {
            await CheckResponse<UserDetailsDTO>(async () => await Client.PutPayloadAsync(Routes.User.Details,
                    new UserDetailsEditDTO(newFirstName, newLastName, newIcon, newPassword), username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newFirstName, content.FirstName);
                    Assert.Equal(newLastName, content.LastName);
                    Assert.Equal(newIcon, content.Icon);
                });

            await CheckResponse<UserDetailsDTO>(async () => await Client.GetAsync(Routes.User.Details, username),
                HttpStatusCode.OK,
                content =>
                {
                    Assert.Equal(username, content.Username);
                    Assert.Equal(newFirstName, content.FirstName);
                    Assert.Equal(newLastName, content.LastName);
                    Assert.Equal(newIcon, content.Icon);
                });
        }
    }
}