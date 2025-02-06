using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using BookwormsServerTesting.Templates;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ChildMgmtTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Theory]
    [InlineData("teacher1")]
    public async Task Test_GetAllChildren_NotParent(string username)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Children.All, username),
            HttpStatusCode.Forbidden,
            ErrorDTO.UserNotParent);
    }

    [Theory]
    [InlineData("parent0")]
    public async Task Test_GetAllChildren_NoChildren(string username)
    {
        HttpResponseMessage response = await Client.GetAsync(Routes.Children.All, username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var children = await response.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
    }

    [Theory]
    [InlineData("joey")]
    public async Task Test_AddChild_NotLoggedIn(string childName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Children.Add(childName)),
            HttpStatusCode.Unauthorized,
            ErrorDTO.Unauthorized);
    }

    [Theory]
    [InlineData("teacher1", "joey")]
    [InlineData("admin", "joey")]
    public async Task Test_AddChild_NotAParent(string username, string childName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Children.Add(childName), username),
            HttpStatusCode.Forbidden,
            ErrorDTO.UserNotParent);
    }

    [Theory]
    [InlineData("parent1", Constants.InvalidChildId, Constants.Parent1Child1Id)]
    public async Task Test_RemoveChild_ChildNotExist(string username, string invalidChildId, string existingChildId)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Children.Remove(invalidChildId), username),
            HttpStatusCode.NotFound,
            ErrorDTO.ChildNotFound);
        
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Single(children);
        Assert.Contains(children, c => c.ChildId == existingChildId);
    }

    [Theory]
    [InlineData("teacher1", Constants.Parent3Child1Id)]
    public async Task Test_RemoveChild_NotParent(string username, string childId)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Children.Remove(childId), username),
            HttpStatusCode.Forbidden,
            ErrorDTO.UserNotParent);
    }

    [Theory]
    [InlineData("teacher1", Constants.Parent3Child1Id)]
    public async Task Test_EditChild_NotParent(string username, string childId)
    {
        await CheckForError(
            () => Client.PutPayloadAsync(Routes.Children.Edit(childId), new ChildEditDTO(), username),
            HttpStatusCode.Forbidden,
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent2", Constants.Parent2Child2Id, "BadVal")]
    public async Task Test_EditChild_InvalidClassroomCode(string username, string childId, string classroomCode)
    {
        await CheckForError(
            () => Client.PutPayloadAsync(Routes.Children.Edit(childId),
                new ChildEditDTO(ClassroomCode: classroomCode, ReadingLevel: "A5"), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorDTO.ClassroomNotFound);
        
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Equal(2, children.Count);
        Assert.Contains(children, c => c.ChildId == childId && c.ClassroomCode is null && c.ReadingLevel is null);
    }

    [Theory]
    [InlineData("parent2", Constants.InvalidChildId, "newName")]
    public async Task Test_EditChild_InvalidId(string username, string childId, string newName)
    {
        await CheckForError(
            () => Client.PutPayloadAsync(Routes.Children.Edit(childId),
                new ChildEditDTO(NewName: newName), username),
            HttpStatusCode.NotFound,
            ErrorDTO.ChildNotFound);
    }
    
    [Theory]
    [InlineData("parent0", "Jason")]
    public async Task Test_AddChild_First(string username, string childName)
    {
        await CheckResponse<List<ChildResponseDTO>>(
            async () => await Client.PostAsync(Routes.Children.Add(childName), username),
            HttpStatusCode.Created,
            (content, headers) => {
                string? childGuid = headers.GetChildLocation();
                Assert.NotNull(childGuid);
                Assert.Single(content);
                Assert.Contains(content, c => c.ChildId == childGuid);
            });
    }
    
    [Theory]
    [InlineData("parent1", "Jason")]
    public async Task Test_AddChild_Second(string username, string childName)
    {
        await CheckResponse<List<ChildResponseDTO>>(
            async () => await Client.PostAsync(Routes.Children.Add(childName), username),
            HttpStatusCode.Created,
            (content, headers) => {
                string? childGuid = headers.GetChildLocation();
                Assert.NotNull(childGuid);
                Assert.Equal(2, content.Count);
                Assert.Contains(content, c => c.ChildId == childGuid);
            });
    }

    [Theory]
    [InlineData("parent0", "joey", "ash")]
    [InlineData("parent0", "joey", "joey")]
    public async Task Test_AddChild_MultipleSameParent(string username, string childName1, string childName2)
    {
        string child1Id = await CheckResponse<List<ChildResponseDTO>, string>(
            async () => await Client.PostAsync(Routes.Children.Add(childName1), username),
            HttpStatusCode.Created,
            (content, headers) => {
                string? childGuid = headers.GetChildLocation();
                Assert.NotNull(childGuid);
                Assert.Single(content);
                Assert.Contains(content, c => c.ChildId == childGuid && c.Name == childName1);
                return childGuid;
            });
        
        string child2Id = await CheckResponse<List<ChildResponseDTO>, string>(
            async () => await Client.PostAsync(Routes.Children.Add(childName2), username),
            HttpStatusCode.Created,
            (content, headers) => {
                string? childGuid = headers.GetChildLocation();
                Assert.NotNull(childGuid);
                Assert.Equal(2, content.Count);
                Assert.Contains(content, c => c.ChildId == childGuid && c.Name == childName2);
                Assert.Contains(content, c => c.ChildId == child1Id && c.Name == childName1);
                return childGuid;
            });
        
        await CheckResponse<List<ChildResponseDTO>>(
            async () => await Client.GetAsync(Routes.Children.All, username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(2, content.Count);
                Assert.Contains(content, c => c.ChildId == child1Id && c.Name == childName1);
                Assert.Contains(content, c => c.ChildId == child2Id && c.Name == childName2);
            });
    }

    [Theory]
    [InlineData("parent0", "parent5", "joey")]
    public async Task Test_AddChild_MultipleSameName_DifferentParents(string username1, string username2,
        string childName)
    {
        await CheckResponse<List<ChildResponseDTO>>(
            async () => await Client.PostAsync(Routes.Children.Add(childName), username1),
            HttpStatusCode.Created,
            content => {
                Assert.Single(content);
                Assert.Contains(content, c => c.Name == childName);
            });
        
        await CheckResponse<List<ChildResponseDTO>>(
            async () => await Client.PostAsync(Routes.Children.Add(childName), username2),
            HttpStatusCode.Created,
            content => {
                Assert.Equal(2, content.Count);
                Assert.Contains(content, c => c.Name != childName);
                Assert.Contains(content, c => c.Name == childName);
            });
        
        List<Child> user1Children = Context.Children.Where(c => c.ParentUsername == username1).ToList();
        Assert.Single(user1Children);
        Assert.Contains(user1Children, c => c.Name == childName);
        
        List<Child> user2Children = Context.Children.Where(c => c.ParentUsername == username2).ToList();
        Assert.Equal(2, user2Children.Count);
        Assert.Contains(user2Children, c => c.Name == childName);
        Assert.Contains(user2Children, c => c.Name != childName);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id)]
    public async Task Test_RemoveChild_Basic(string username, string childId)
    {
        await CheckResponse<List<ChildResponseDTO>>(
            async () => await Client.DeleteAsync(Routes.Children.Remove(childId), username),
            HttpStatusCode.OK,
            Assert.Empty);
        
        Assert.Empty(Context.Children.Where(c => c.ParentUsername == username));
    }

    [Theory]
    [InlineData("parent2", Constants.Parent2Child2Id, Constants.Parent2Child1Id)]
    public async Task Test_RemoveChild_DoesNotDeleteOtherChildren(string username, string childToRemoveId, string childLeftId)
    {
        await CheckResponse<List<ChildResponseDTO>>(
            async () => await Client.DeleteAsync(Routes.Children.Remove(childToRemoveId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.Contains(content, c => c.ChildId == childLeftId);
            });
        
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Single(children);
        Assert.Contains(children, c => c.ChildId == childLeftId);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Rachel")]
    public async Task Test_EditChild_ChangeName_Basic(string username, string childId, string newName)
    {
        await CheckResponse<ChildResponseDTO>(
            async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId), 
                new ChildEditDTO(newName), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(newName, content.Name);
            });
        
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Single(children);
        Assert.Contains(children, c => c.ChildId == childId && c.Name == newName);
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child2Id, 2)]
    public async Task Test_EditChild_ChangeIcon_Basic(string username, string childId, int newIcon)
    {
        await CheckResponse<ChildResponseDTO>(
            async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId),
            new ChildEditDTO(ChildIcon: newIcon), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(childId, content.ChildId);
                Assert.Equal(newIcon, content.ChildIcon);
            });
        
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Equal(3, children.Count);
        Assert.Contains(children, c => c.ChildId == childId && c.ChildIcon == newIcon);
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child2Id, Constants.Parent3Child3Id,
        "Costanza")]
    public async Task Test_EditChild_ChangeName_NameAlreadyExistsUnderParent(string username, string child1Id, string child2Id, string newName)
    {
        await CheckResponse<ChildResponseDTO>(
            async () => await Client.PutPayloadAsync(Routes.Children.Edit(child1Id),
            new ChildEditDTO(NewName: newName), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(child1Id, content.ChildId);
                Assert.Equal(newName, content.Name);
            });
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Equal(3, children.Count);
        Assert.Contains(children, c => c.ChildId == child1Id && c.Name == newName);
        Assert.Contains(children, c => c.ChildId == child2Id && c.Name == newName);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "A4")]
    public async Task Test_EditChild_ChangeReadingLevel_Basic(string username, string childId, string readingLevel)
    {
        await CheckResponse<ChildResponseDTO>(
            async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId),
            new ChildEditDTO(ReadingLevel: readingLevel), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(childId, content.ChildId);
                Assert.Equal(readingLevel, content.ReadingLevel);
            });
        
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Single(children);
        Assert.Contains(children, c => c.ChildId == childId && c.ReadingLevel == readingLevel);
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child3Id, "1960-08-31")]
    public async Task Test_EditChild_ChangeDOB_Basic(string username, string childId, string dob)
    {
        await CheckResponse<ChildResponseDTO>(
            async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId),
            new ChildEditDTO(DateOfBirth: DateOnly.Parse(dob)), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(childId, content.ChildId);
                Assert.Equal(DateOnly.Parse(dob), content.DateOfBirth);
            });
        
        List<Child> children = Context.Children.Where(c => c.ParentUsername == username).ToList();
        Assert.Equal(3, children.Count);
        Assert.Contains(children, c => c.ChildId == childId && c.DateOfBirth == DateOnly.Parse(dob));
    }

    // TODO - Add more classroom code edit validation when classrooms are added

}