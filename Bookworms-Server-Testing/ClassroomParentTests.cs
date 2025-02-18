using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ClassroomParentTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Theory]
    [InlineData("2bc5ae6239c988", "ABC123")]
    public async Task Test_ChildClassroomRoutes_NotLoggedIn(string childId, string classCode)
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.Classrooms.All(childId)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.Join(childId, classCode)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.Leave(childId, classCode)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("admin", "2bc5ae6239c988", "ABC123")]
    [InlineData("teacher1", "2bc5ae6239c988", "ABC123")]
    public async Task Test_ChildClassroomRoutes_NotParent_ShouldForbid(string username, string childId, string classCode)
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.Classrooms.All(childId), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.Join(childId, classCode), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.Leave(childId, classCode), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent1", "2bc5ae6239c988", "NOTFND")]
    public async Task Test_JoinOrLeaveClass_WrongCode_ShouldNotFound(string username, string childId, string classCode)
    {
        await CheckResponse<ErrorResponse>(
            async () => await Client.PostAsync(Routes.Classrooms.Join(childId, classCode), username),
            HttpStatusCode.NotFound,
            content => {
                Assert.Equal(content, ErrorResponse.ClassroomNotFound);
            });
        
        await CheckResponse<ErrorResponse>(
            async () => await Client.DeleteAsync(Routes.Classrooms.Leave(childId, classCode), username),
            HttpStatusCode.NotFound,
            content => {
                Assert.Equal(content, ErrorResponse.ClassroomNotFound);
            });
    }
    
    [Theory]
    [InlineData("parent1", "badId", "ABC123")]
    [InlineData("parent2", "2bc5ae6239c988", "ABC123")]
    public async Task Test_ChildClassroomRoutes_WrongParentOrInvalidChildId_ShouldNotFound(string username, string childId, string classCode)
    {
        await CheckResponse<ErrorResponse>(
            async () => await Client.GetAsync(Routes.Classrooms.All(childId), username),
            HttpStatusCode.NotFound,
            content => {
                Assert.Equal(content, ErrorResponse.ChildNotFound);
            });
        
        await CheckResponse<ErrorResponse>(
            async () => await Client.PostAsync(Routes.Classrooms.Join(childId, classCode), username),
            HttpStatusCode.NotFound,
            content => {
                Assert.Equal(content, ErrorResponse.ChildNotFound);
            });
        
        await CheckResponse<ErrorResponse>(
            async () => await Client.DeleteAsync(Routes.Classrooms.Leave(childId, classCode), username),
            HttpStatusCode.NotFound,
            content => {
                Assert.Equal(content, ErrorResponse.ChildNotFound);
            });
    }
    
    [Theory]
    [InlineData("parent1", "2bc5ae6239c988", "BBB222")]
    public async Task Test_JoinClass_ChildAlreadyInClass_ShouldError(string username, string childId, string classCode)
    {
        await CheckResponse<ErrorResponse>(
            async () => await Client.PostAsync(Routes.Classrooms.Join(childId, classCode), username),
            HttpStatusCode.UnprocessableEntity,
            content => {
                Assert.Equal(content, ErrorResponse.ChildAlreadyInClass);
            });
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b3b701b2ee", "BBB222")]
    public async Task Test_LeaveClass_ChildNotInClass_ShouldError(string username, string childId, string classCode)
    {
        await CheckResponse<ErrorResponse>(
            async () => await Client.DeleteAsync(Routes.Classrooms.Leave(childId, classCode), username),
            HttpStatusCode.NotFound,
            content => {
                Assert.Equal(content, ErrorResponse.ChildNotInClass);
            });
    }
    
    [Theory]
    [InlineData("parent2", "2bc5b121b46b4a", 0)]
    [InlineData("parent1", "2bc5ae6239c988", 1)]
    [InlineData("parent3", "2bc5b2a4771d7f", 2)]
    public async Task Test_GetAllClassrooms(string username, string childId, int numClasses)
    {
        await CheckResponse<List<ClassroomChildResponse>>(
            async () => await Client.GetAsync(Routes.Classrooms.All(childId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(numClasses, content.Count);
            });
    }
    
    [Theory]
    [InlineData("parent5", "2bc5b4ec4f452c", "ABC123", 1)]
    [InlineData("parent4", "2bc5b4544b34e7", "ABC123", 2)]
    [InlineData("parent3", "2bc5b2a4771d7f", "ABC123", 3)]
    public async Task Test_JoinClassroom(string username, string childId, string classCode, int numClasses)
    {
        await CheckResponse<ClassroomChildResponse>(
            async () => await Client.PostAsync(Routes.Classrooms.Join(childId, classCode), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(2, content.Bookshelves.Count);
                foreach (BookshelfResponse bookshelf in content.Bookshelves)
                {
                    Assert.Single(bookshelf.Books);
                }
            });
        
        Assert.True(Context.ClassroomChildren.Contains(new(classCode, childId)));
        Assert.Equal(numClasses, Context.Children
            .Include(c => c.Classrooms)
            .First(c => c.ChildId == childId).Classrooms.Count);
    }
    
    [Theory]
    [InlineData("parent1", "2bc5ae6239c988", "BBB222", 0)]
    [InlineData("parent3", "2bc5b2a4771d7f", "BBB222", 1)]
    public async Task Test_LeaveClassroom(string username, string childId, string classCode, int numClasses)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.Classrooms.Leave(childId, classCode), username),
            HttpStatusCode.NoContent);

        Assert.False(Context.ClassroomChildren.Contains(new(classCode, childId)));
        Assert.Equal(numClasses, Context.Children
            .Include(c => c.Classrooms)
            .First(c => c.ChildId == childId).Classrooms.Count);
    }
    
    [Theory]
    [InlineData("parent4", "2bc5b4544b34e7", "ZETA28")]
    public async Task Test_LeaveClassroom_LastChild(string username, string childId, string classCode)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.Classrooms.Leave(childId, classCode), username),
            HttpStatusCode.NoContent);

        Assert.False(Context.ClassroomChildren.Contains(new(classCode, childId)));
        Assert.Empty(Context.Classrooms
            .Include(c => c.Children)
            .First(c => c.ClassroomCode == classCode).Children);
    }
}