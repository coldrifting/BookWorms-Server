using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ClassroomAnnouncementTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Fact]
    public async Task Test_TeacherClassroomAnnouncementRoutes_NotLoggedIn()
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.ClassAnnouncements.All),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Add, 
                new ClassroomAnnouncementAddRequest("Title", "Body")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Edit("blah"), 
                new ClassroomAnnouncementEditRequest("Title", "Body")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Delete("blah")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Clear),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("parent1")]
    public async Task Test_TeacherClassroomAnnouncementRoutes_NotTeacher(string username)
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.ClassAnnouncements.All, username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Add, 
                new ClassroomAnnouncementAddRequest("Title", "Body"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Edit("blah"), 
                new ClassroomAnnouncementEditRequest("Title", "Body"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Delete("blah"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Clear, username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
    }

    [Theory]
    [InlineData("teacher0")]
    public async Task Test_TeacherClassroomAnnouncementRoutes_NoClassroom(string username)
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.ClassAnnouncements.All, username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Add, 
                new ClassroomAnnouncementAddRequest("Title", "Body"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Edit("blah"), 
                new ClassroomAnnouncementEditRequest("Title", "Body"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Delete("blah"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Clear, username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }

    [Theory]
    [InlineData("teacher1", "invalidId")]
    [InlineData("teacher1", "Announce3")]
    public async Task Test_TeacherClassroomAnnouncementRoutes_InvalidAnnouncementId(string username, string announcementId)
    {
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Edit(announcementId), 
                new ClassroomAnnouncementEditRequest("Title", "Body"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomAnnouncementNotFound);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Delete(announcementId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomAnnouncementNotFound);
    }

    [Theory]
    [InlineData("teacher1", "Announce1", "Announcement 1", "Some Text 1", 1)]
    [InlineData("teacher2", "Announce2", "Announcement 2", "Some Text 2", 2)]
    [InlineData("teacher2", "Announce3", "Announcement 3", "Some Text 3", 2)]
    public async Task Test_TeacherClassroomAnnouncement_All_Basic(string username, string containedId, string containedTitle, string containedBody, int numRemaining)
    {
        await CheckResponse<List<ClassroomAnnouncementResponse>>(
            async () => await Client.GetAsync(Routes.ClassAnnouncements.All, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(numRemaining, content.Count);
                Assert.Contains(content, c => c.AnnouncementId == containedId && c.Title == containedTitle && c.Body == containedBody);
            });
    }

    [Theory]
    [InlineData("teacher1", "T1", "B1", 2)]
    [InlineData("teacher2", "T2", "B2", 3)]
    [InlineData("teacher2", "T3", "B3", 3)]
    public async Task Test_TeacherClassroomAnnouncement_Add_Basic(string username, string newTitle, string newBody, int numRemaining)
    {
        await CheckResponse<ClassroomAnnouncementResponse>(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Add, 
                new ClassroomAnnouncementAddRequest(newTitle, newBody), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(newTitle, content.Title);
                Assert.Equal(newBody, content.Body);
            });

        var announcements = Context.ClassroomAnnouncements
            .Include(ca => ca.Classroom)
            .Where(ca => ca.Classroom.TeacherUsername == username).ToList();

        Assert.Equal(numRemaining, announcements.Count);
        Assert.Contains(announcements, c => c.Title == newTitle && c.Body == newBody);
    }

    [Theory]
    [InlineData("teacher1", "Announce1", "T1", "B1", 1)]
    [InlineData("teacher2", "Announce2", "T2", "B2", 2)]
    [InlineData("teacher2", "Announce3", "T3", "B3", 2)]
    public async Task Test_TeacherClassroomAnnouncement_Edit_Basic(string username, string id, string newTitle, string newBody, int numRemaining)
    {
        await CheckResponse<ClassroomAnnouncementResponse>(
            async () => await Client.PutPayloadAsync(Routes.ClassAnnouncements.Edit(id), 
                new ClassroomAnnouncementEditRequest(newTitle, newBody), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(newTitle, content.Title);
                Assert.Equal(newBody, content.Body);
            });

        var announcements = Context.ClassroomAnnouncements
            .Include(ca => ca.Classroom)
            .Where(ca => ca.Classroom.TeacherUsername == username).ToList();

        Assert.Equal(numRemaining, announcements.Count);
        Assert.Contains(announcements, c => c.AnnouncementId == id && 
                                            c.Title == newTitle && c.Body == newBody);
    }

    [Theory]
    [InlineData("teacher1", "Announce1", 0)]
    [InlineData("teacher2", "Announce2", 1)]
    [InlineData("teacher2", "Announce3", 1)]
    public async Task Test_TeacherClassroomAnnouncement_Delete_Basic(string username, string id, int numRemaining)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Delete(id), username),
            HttpStatusCode.NoContent);

        var announcements = Context.ClassroomAnnouncements
            .Include(ca => ca.Classroom)
            .Where(ca => ca.Classroom.TeacherUsername == username).ToList();

        Assert.Equal(numRemaining, announcements.Count);
        Assert.DoesNotContain(announcements, c => c.AnnouncementId == id);
    }

    [Theory]
    [InlineData("teacher1")]
    [InlineData("teacher2")]
    public async Task Test_TeacherClassroomAnnouncement_Clear_Basic(string username)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.ClassAnnouncements.Clear, username),
            HttpStatusCode.NoContent);
        
        Assert.Empty(Context.ClassroomAnnouncements
            .Include(ca => ca.Classroom)
            .Where(ca => ca.Classroom.TeacherUsername == username));
    }
}