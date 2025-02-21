using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ClassroomGoalTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    private const string GoalId = "goalId";
    private static readonly ClassGoalAddRequest ClassGoalAddCompletionRequest = new ("Completion Goal", DateOnly.Parse("2025-05-01"), null);
    private static readonly ClassGoalEditRequest ClassGoalEditCompletionRequest = new ("Edit Goal Completion", DateOnly.Parse("2025-05-01"), null);

    [Fact]
    public async Task Test_ClassGoalRoutes_NotLoggedIn()
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.ClassGoals.All),
                async () => await Client.PostPayloadAsync(Routes.ClassGoals.Add, new {}),
                async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(GoalId), new {}),
                async () => await Client.DeleteAsync(Routes.ClassGoals.Delete(GoalId)),
                async () => await Client.GetAsync(Routes.ClassGoals.Details(GoalId)),
                async () => await Client.GetAsync(Routes.ClassGoals.DetailsAll(GoalId))
            ],
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("admin")]
    [InlineData("parent1")]
    public async Task Test_ClassGoalRoutes_NotTeacher(string username)
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.ClassGoals.All, username),
                async () => await Client.PostPayloadAsync(Routes.ClassGoals.Add, ClassGoalAddCompletionRequest, username),
                async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(GoalId), ClassGoalEditCompletionRequest, username),
                async () => await Client.DeleteAsync(Routes.ClassGoals.Delete(GoalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.Details(GoalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.DetailsAll(GoalId), username)
            ],
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
    }
    
    [Theory]
    [InlineData("teacher0")]
    public async Task Test_ClassGoalRoutes_NoClassCreated(string username)
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.ClassGoals.All, username),
                async () => await Client.PostPayloadAsync(Routes.ClassGoals.Add, ClassGoalAddCompletionRequest, username),
                async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(GoalId), ClassGoalEditCompletionRequest, username),
                async () => await Client.DeleteAsync(Routes.ClassGoals.Delete(GoalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.Details(GoalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.DetailsAll(GoalId), username)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }
    
    [Theory]
    [InlineData("teacher3")]
    public async Task Test_ClassGoalRoutes_NoGoalsCreated(string username)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(GoalId), new {}, username),
                async () => await Client.DeleteAsync(Routes.ClassGoals.Delete(GoalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.Details(GoalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.DetailsAll(GoalId), username)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("teacher2", "413b217f330ce8")]
    public async Task Test_ClassGoalRoutes_GoalFromOtherClass(string username, string goalId)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(goalId), new {}, username),
                async () => await Client.DeleteAsync(Routes.ClassGoals.Delete(goalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.Details(goalId), username),
                async () => await Client.GetAsync(Routes.ClassGoals.DetailsAll(goalId), username)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }

    [Fact]
    public async Task Test_AllClassGoals_Basic()
    {
        await CheckResponse<ClassGoalOverviewResponse>(async () => await Client.GetAsync(Routes.ClassGoals.All, "teacher2"), HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(9, content.CompletionGoals.Count + content.NumBookGoals.Count);
                
                foreach (var v in content.CompletionGoals)
                {
                    Assert.Equal(2, v.TotalStudents);
                }
                foreach (var v in content.NumBookGoals)
                {
                    Assert.Equal(2, v.TotalStudents);
                }
                
                Assert.Contains(content.CompletionGoals, c => c is { GoalId: "413a8732533462", StudentsCompleted: 2, AverageCompletionTime: 37 });
                Assert.Contains(content.CompletionGoals, c => c is { GoalId: "413a8732545964", StudentsCompleted: 1, AverageCompletionTime: 50 });
                Assert.Contains(content.CompletionGoals, c => c is { GoalId: "413a8732584729", StudentsCompleted: 1, AverageCompletionTime: 25 });
                Assert.Contains(content.CompletionGoals, c => c is { GoalId: "413a8732581806", StudentsCompleted: 0, AverageCompletionTime: null });
                Assert.Contains(content.CompletionGoals, c => c is { GoalId: "413a8732516249", StudentsCompleted: 0, AverageCompletionTime: null });
                
                Assert.Contains(content.NumBookGoals, c => c is { GoalId: "413b1d8ae564c2", TargetNumBooks: 1, StudentsCompleted: 2, AverageBooksRead: 1.5 });
                Assert.Contains(content.NumBookGoals, c => c is { GoalId: "413b1d8ae99498", TargetNumBooks: 3, StudentsCompleted: 1, AverageBooksRead: 2 });
                Assert.Contains(content.NumBookGoals, c => c is { GoalId: "413b1d8ae65108", TargetNumBooks: 2, StudentsCompleted: 1, AverageBooksRead: 2 });
                Assert.Contains(content.NumBookGoals, c => c is { GoalId: "413b1d8ae55090", TargetNumBooks: 2, StudentsCompleted: 0, AverageBooksRead: 1 });
            });
        
        await CheckResponse<ClassGoalOverviewResponse>(async () => await Client.GetAsync(Routes.ClassGoals.All, "teacher4"), HttpStatusCode.OK,
            content =>
            {
                Assert.Empty(content.CompletionGoals);
                Assert.Single(content.NumBookGoals);
                
                Assert.Contains(content.NumBookGoals, c => c is { GoalId: "413b217f330ce8", TotalStudents: 1, StudentsCompleted: 0, AverageBooksRead: null });
            });
    }
    
    // Goal Details (Basic & All, All 10 routes)
    [Theory]
    [InlineData("teacher2", "413a8732533462", 2, 2, 37, null, null)]
    [InlineData("teacher2", "413a8732545964", 2, 1, 50, null, null)]
    [InlineData("teacher2", "413a8732584729", 2, 1, 25, null, null)]
    [InlineData("teacher2", "413a8732581806", 2, 0, null, null, null)]
    [InlineData("teacher2", "413a8732516249", 2, 0, null, null, null)]
    [InlineData("teacher2", "413b1d8ae564c2", 2, 2, null, 1, 1.5)]
    [InlineData("teacher2", "413b1d8ae99498", 2, 1, null, 3, 2.0)]
    [InlineData("teacher2", "413b1d8ae65108", 2, 1, null, 2, 2.0)]
    [InlineData("teacher2", "413b1d8ae55090", 2, 0, null, 2, 1.0)]
    public async Task Test_Details(string username, string goalId, int totalStudents, int studentsCompleted, int? averageCompletionTime, int? targetNumBooks, double? averageBooksRead )
    {
        await CheckResponse<ClassGoalResponse>(
            async () => await Client.GetAsync(Routes.ClassGoals.Details(goalId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(totalStudents, content.TotalStudents);
                Assert.Equal(studentsCompleted, content.StudentsCompleted);
                if (content is ClassGoalCompletionResponse contentCompletion)
                {
                    Assert.Equal(averageCompletionTime, contentCompletion.AverageCompletionTime);
                }
                else if (content is ClassGoalNumBooksResponse contentNumBooks)
                {
                    Assert.Equal(targetNumBooks, contentNumBooks.TargetNumBooks);
                    Assert.Equal(averageBooksRead, contentNumBooks.AverageBooksRead);
                }
            });
        
        await CheckResponse<ClassGoalDetailedResponse>(
            async () => await Client.GetAsync(Routes.ClassGoals.DetailsAll(goalId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(totalStudents, content.TotalStudents);
                Assert.Equal(studentsCompleted, content.StudentsCompleted);
                if (content is ClassGoalCompletionDetailedResponse contentCompletion)
                {
                    Assert.Equal(averageCompletionTime, contentCompletion.AverageCompletionTime);
                }
                else if (content is ClassGoalNumBooksDetailedResponse contentNumBooks)
                {
                    Assert.Equal(targetNumBooks, contentNumBooks.TargetNumBooks);
                    Assert.Equal(averageBooksRead, contentNumBooks.AverageBooksRead);
                }

                Assert.Equal(totalStudents, content.StudentGoalStatus.Count);
                Assert.Equal(studentsCompleted, content.StudentGoalStatus.Count(s => s.HasAchievedGoal));
                foreach (var s in content.StudentGoalStatus)
                {
                    Assert.NotNull(s.ChildName);
                }
            });
    }
    
    // New Goal
    [Theory]
    [InlineData("teacher2")]
    public async Task Test_AddGoal(string username)
    {
        ClassGoalAddRequest requestCompletion = new("Complete", DateOnly.Parse("2025-01-01"));
        var goalId1 = await CheckResponse<ClassGoalCompletionResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.ClassGoals.Add, requestCompletion, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal("Complete", content.Title);
                return content.GoalId;
            });

        var gc = await Context.ClassGoals.FindAsync(goalId1);
        Assert.NotNull(gc);
        Assert.Equal("Complete", gc.Title);
        Assert.Equal(10, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        
        ClassGoalAddRequest requestNumBooks = new("NumBooks", DateOnly.Parse("2025-02-02"));
        var goalId2 = await CheckResponse<ClassGoalNumBooksResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.ClassGoals.Add, requestNumBooks, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal("NumBooks", content.Title);
                return content.GoalId;
            });

        Assert.Equal(11, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        var gn = await Context.ClassGoals.FindAsync(goalId2);
        Assert.NotNull(gn);
        Assert.Equal("NumBooks", gn.Title);
    }

    // Edit Details (Num & Completion)
    [Theory]
    [InlineData("teacher2", "413a8732516249", "NewComplete")]
    [InlineData("teacher2", "413b1d8ae55090", "NewNumBooks")]
    public async Task Test_EditGoalTitle(string username, string goalId, string newTitle)
    {
        ClassGoalEditRequest editRequest = new(newTitle, null, null);
        
        await CheckResponse<ClassGoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(newTitle, content.Title);
            });

        Assert.Equal(9, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        ClassGoal? g = await Context.ClassGoals.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(newTitle, g.Title);
    }
    
    [Theory]
    [InlineData("teacher2", "413a8732516249", "2023-02-17")]
    [InlineData("teacher2", "413b1d8ae55090", "2013-10-04")]
    public async Task Test_EditGoalEndDate(string username, string goalId, string newEndDate)
    {
        ClassGoalEditRequest editRequest = new(null, DateOnly.Parse(newEndDate), null);
        
        await CheckResponse<ClassGoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(DateOnly.Parse(newEndDate), content.EndDate);
            });

        Assert.Equal(9, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        ClassGoal? g = await Context.ClassGoals.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(DateOnly.Parse(newEndDate), g.EndDate);
    }
    
    [Theory]
    [InlineData("teacher2", "413a8732516249", 5)]
    [InlineData("teacher2", "413b1d8ae55090", 7)]
    public async Task Test_EditGoalTargetNumBooks(string username, string goalId, int targetNumBooks)
    {
        ClassGoalEditRequest editRequest = new(null, null, targetNumBooks);
        
        await CheckResponse<ClassGoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.Edit(goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                if (content is ClassGoalNumBooksResponse numBooksContent)
                {
                    Assert.Equal(targetNumBooks, numBooksContent.TargetNumBooks);
                }
            });

        Assert.Equal(9, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        ClassGoal? g = await Context.ClassGoals.FindAsync(goalId);
        Assert.NotNull(g);

        // Should succeed but do nothing if goal type is completion metric
        if (g is ClassGoalNumBooks gn)
        {
            Assert.Equal(targetNumBooks, gn.TargetNumBooks);
        }
    }
    
    // Delete Goal (Both Types)
    [Theory]
    [InlineData("teacher2", "413a8732516249")]
    [InlineData("teacher2", "413b1d8ae55090")]
    public async Task Test_DeleteGoal(string username, string goalId)
    {
        await CheckResponse<ClassGoalOverviewResponse>(
            async () => await Client.DeleteAsync(Routes.ClassGoals.Delete(goalId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(8, content.CompletionGoals.Count + content.NumBookGoals.Count);
                Assert.DoesNotContain(content.CompletionGoals, c => c.GoalId == goalId);
                Assert.DoesNotContain(content.NumBookGoals, c => c.GoalId == goalId);
            });
        
        Assert.Null(await Context.ClassGoals.FindAsync(goalId));
    }
    
    // Child Class Goal Update Tests 

    [Theory]
    [InlineData(Constants.InvalidChildId, "ABC123", "InvalidGoalId")]
    public async Task Test_UpdateClassGoalLog_NotLoggedIn(string childId, string classCode, string goalId)
    {
        await CheckForError(async () => await Client.PutAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("admin", Constants.InvalidChildId, "ABC123", "InvalidGoalId")]
    [InlineData("teacher1", Constants.InvalidChildId, "ABC123", "InvalidGoalId")]
    public async Task Test_UpdateClassGoalLog_NotParent(string username, string childId, string classCode, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), new {}, username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent1", Constants.InvalidChildId, "ABC123", "InvalidGoalId")]
    [InlineData("parent2", Constants.Parent1Child1Id, "ABC123", "InvalidGoalId")]
    public async Task Test_UpdateClassGoalLog_ChildNotExistOrWrongParent(string username, string childId, string classCode, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), new {}, username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }
    
    [Theory]
    [InlineData("parent2", Constants.Parent2Child2Id, "NOCODE", "InvalidGoalId")]
    public async Task Test_UpdateClassGoalLog_ClassNotExist(string username, string childId, string classCode, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), new {}, username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }
    
    [Theory]
    [InlineData("parent3", Constants.Parent3Child1Id, "ABC123", "InvalidGoalId")]
    public async Task Test_UpdateClassGoalLog_ChildNotInClass(string username, string childId, string classCode, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId),new {},  username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotInClass);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "BBB222", "InvalidGoalId")]
    public async Task Test_UpdateClassGoalLog_ClassGoalNotExist(string username, string childId, string classCode, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId),new {},  username),
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "BBB222", "413b16cf615d35")]
    public async Task Test_UpdateClassGoalLog_ClassGoalWrongClass(string username, string childId, string classCode, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId),new {},  username),
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "BBB222", "413a8732581806")]
    public async Task Test_UpdateClassGoalLog_NewCompletion(string username, string childId, string classCode, string goalId)
    {
        ClassGoalLogEditRequest request = new(NumBooks: 5);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Duration: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0, Duration: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Progress: 0, Duration: 1);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Progress: 1, Duration: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Progress: 1);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Duration: 10);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0.5f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.OK,
            Assert.False);
        
        request = new(Progress: 1f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.OK,
            Assert.True);
        
        // Should ignore extra fields
        request = new(NumBooks: 5, Progress: 1f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.OK,
            Assert.True);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "BBB222", "413b1d8ae65108")]
    public async Task Test_UpdateClassGoalLog_NewNumBooks(string username, string childId, string classCode, string goalId)
    {
        ClassGoalLogEditRequest request = new(Progress: 1);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(NumBooks: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Duration: 10);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0.5f, Duration: 10);
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(NumBooks: 1);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.OK,
            Assert.False);
        
        request = new(NumBooks: 2);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.OK,
            content => Assert.True(content));
        
        request = new(NumBooks: 3);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.OK,
            Assert.True);
        
        // Should ignore extra fields
        request = new(NumBooks: 1, Progress: 1f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ClassGoals.UpdateLog(childId, classCode, goalId), request, username),
            HttpStatusCode.OK,
            Assert.False);
    }
    
    // TODO - Add more when more child class goal related routes are added
}