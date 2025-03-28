using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class GoalParentTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    private const string SomeGoalId = "goalId";
    private const string SomeChildId = "childId";
    
    private const string ChildCompletionGoalId = "4aa30ee8b6d4e3";
    private const string ChildBooksReadGoalId = "4aa48f227d01fd";
    
    
    private static readonly GoalAddRequest NewChildBooksReadGoal = new(
        GoalType.Child, 
        GoalMetric.BooksRead,
        "ChildBooksReadGoalTitle", 
        DateOnly.Parse("2025-01-01"), 
        DateOnly.Parse("2025-02-02"), 
        1);
    
    private static readonly GoalAddRequest NewClassCompletionGoal = new(
        GoalType.Classroom, 
        GoalMetric.Completion,
        "ClassCompletionGoalTitle", 
        DateOnly.Parse("2025-01-01"), 
        DateOnly.Parse("2025-02-02"), 
        1);
    
    private static readonly GoalAddRequest NewClassAggregateMinutesRead = new(
        GoalType.ClassroomAggregate, 
        GoalMetric.MinutesRead,
        "ClassAggregateMinutesReadGoalTitle", 
        DateOnly.Parse("2025-01-01"), 
        DateOnly.Parse("2025-02-02"), 
        1);
    
    [Fact]
    public async Task Test_ChildGoalRoutes_NotLoggedIn()
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.Children.Goals.All(SomeChildId)),
                async () => await Client.PostPayloadAsync(Routes.Children.Goals.Add(SomeChildId), new {}),
                async () => await Client.GetAsync(Routes.Children.Goals.Details(SomeGoalId, SomeChildId)),
                async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(SomeGoalId, SomeChildId), new {}),
                async () => await Client.PutPayloadAsync(Routes.Children.Goals.Log(SomeGoalId, SomeChildId, 5), new {}),
                async () => await Client.DeleteAsync(Routes.Children.Goals.Delete(SomeGoalId, SomeChildId)),
            ],
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("admin")]
    [InlineData("teacher0")]
    public async Task Test_ChildGoalRoutes_NotParent(string username)
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.Children.Goals.All(SomeChildId), username),
                async () => await Client.PostPayloadAsync(Routes.Children.Goals.Add(SomeChildId), NewChildBooksReadGoal, username),
                async () => await Client.GetAsync(Routes.Children.Goals.Details(SomeChildId, ChildCompletionGoalId), username),
                async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(SomeChildId, ChildBooksReadGoalId), new GoalEditRequest(Title: "Title"), username),
                async () => await Client.PutAsync(Routes.Children.Goals.Log(SomeChildId, SomeGoalId, 5), username),
                async () => await Client.DeleteAsync(Routes.Children.Goals.Delete(SomeChildId, SomeGoalId), username)
            ],
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b2a4771d7f")]
    public async Task Test_ChildGoalRoutes_NoGoalsCreated(string username, string childId)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(childId, SomeGoalId), new GoalEditRequest(Title: "Title"), username),
                async () => await Client.DeleteAsync(Routes.Children.Goals.Delete(childId, SomeGoalId), username),
                async () => await Client.GetAsync(Routes.Children.Goals.Details(childId, SomeGoalId), username)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("parent2", "2bc5b3b701b2ee", "4aa310dc9dd437")]
    public async Task Test_ChildGoalRoutes_GoalFromOtherChild(string username, string childId, string goalId)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(childId, goalId), new GoalEditRequest(Title: "Title"), username),
                async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 5), username),
                async () => await Client.DeleteAsync(Routes.Children.Goals.Delete(childId, goalId), username),
                async () => await Client.GetAsync(Routes.Children.Goals.Details(childId, goalId), username),
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData("parent2", Constants.Parent1Child1Id)]
    public async Task Test_AllGoals_WrongChild(string username, string childId)
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.Children.Goals.All(childId),
                username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound
        );
    }
    
    [Theory]
    [InlineData("parent1", "2bc5ae6239c988", "parent3", "2bc5b325697a55")]
    public async Task Test_AllChildGoals_Basic(string username1, string childId1, string username2, string childId2)
    {
        await CheckResponse<List<GoalResponse>>(async () => await Client.GetAsync(Routes.Children.Goals.All(childId1), username1), HttpStatusCode.OK,
            content =>
            {
                
                Assert.Equal(6, content.Count(g => g.GoalMetric == GoalMetric.Completion));
                Assert.Equal(5, content.Count(g => g.GoalMetric == GoalMetric.BooksRead));
                
                Assert.Equal(11, content.Count);
                
                Assert.Contains(content, g => g is { GoalType: GoalType.Child, GoalMetric: GoalMetric.Completion, GoalId: "4aa30907023f3b", Progress: 0 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Child, GoalMetric: GoalMetric.BooksRead, GoalId: "4aa48941faee41", Target: 2, Progress: 0 });
                
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.Completion, GoalId: "413a8732533462", Progress: 37100 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.Completion, GoalId: "413a8732545964", Progress: 37057 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.Completion, GoalId: "413a8732584729", Progress: 12050 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.Completion, GoalId: "413a8732516249", Progress: 12025 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.Completion, GoalId: "413a8732581806", Progress: 0 });
                
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.BooksRead, GoalId: "413b1d8ae564c2", Target: 1, Progress: 1 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.BooksRead, GoalId: "413b1d8ae99498", Target: 3, Progress: 2 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.BooksRead, GoalId: "413b1d8ae65108", Target: 2, Progress: 1 });
                Assert.Contains(content, g => g is { GoalType: GoalType.Classroom, GoalMetric: GoalMetric.BooksRead, GoalId: "413b1d8ae55090", Target: 2, Progress: 0 });
            });
        
        await CheckResponse<List<GoalResponse>>(async () => await Client.GetAsync(Routes.Children.Goals.All(childId2), username2), HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(2, content.Count);
                
                Assert.Equal(1, content.Count(g => g is { GoalType: GoalType.Child, GoalMetric: GoalMetric.Completion, GoalId: "4aa30cf4a23193", Progress: 2050 }));
                Assert.Equal(1, content.Count(g => g is { GoalType: GoalType.Child, GoalMetric: GoalMetric.BooksRead, GoalId: "4aa48d2e0e7a77", Target: 2, Progress: 2 }));
            });
    }
    
    
    // Goal Details (all 12 test child goals)
    
    [Theory]
    [InlineData("parent1", "2bc5ae6239c988", "4aa30907023f3b", GoalMetric.Completion, 0)]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa30b00417f9f", GoalMetric.Completion, 5000)]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", GoalMetric.Completion, 2050)]
    [InlineData("parent4", "2bc5b4544b34e7", "4aa30ee8b6d4e3", GoalMetric.Completion, 4050)]
    [InlineData("parent3", "2bc5b3b701b2ee", "4aa310dc9dd437", GoalMetric.Completion, 2001)]
    [InlineData("parent5", "2bc5b4ec4f452c", "4aa312d0e19e10", GoalMetric.Completion, 20001)]
    [InlineData("parent1", "2bc5ae6239c988", "4aa48941faee41", GoalMetric.BooksRead, 0, 2)]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", GoalMetric.BooksRead, 1, 2)]
    [InlineData("parent3", "2bc5b325697a55", "4aa48d2e0e7a77", GoalMetric.BooksRead, 2, 2)]
    [InlineData("parent4", "2bc5b4544b34e7", "4aa48f227d01fd", GoalMetric.BooksRead, 0, 20)]
    [InlineData("parent3", "2bc5b3b701b2ee", "4aa49116545ede", GoalMetric.BooksRead, 10, 20)]
    [InlineData("parent5", "2bc5b4ec4f452c", "4aa4930a9675ae", GoalMetric.BooksRead, 20, 20)]
    public async Task Test_Details(string username, string childId, string goalId, GoalMetric goalMetric, int progress, int target = 0 )
    {
        await CheckResponse<GoalResponse>(
            async () => await Client.GetAsync(Routes.Children.Goals.Details(childId, goalId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(GoalType.Child, content.GoalType);
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(goalMetric, content.GoalMetric);
                Assert.Equal(progress, content.Progress);
                Assert.Equal(target, content.Target);
            });
    }
    
    
    // New Goal

    [Theory]
    [InlineData("parent2", Constants.Parent1Child1Id)]
    public async Task Test_AddGoal_WrongChild(string username, string childId)
    {
        await CheckForError(
            async () => await Client.PostPayloadAsync(Routes.Children.Goals.Add(childId),
                new GoalAddRequest(GoalType.Child,
                    GoalMetric.Completion,
                    "Title",
                    DateOnly.Parse("2025-01-01"),
                    DateOnly.Parse("2025-02-02"),
                    1),
                username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound
        );
    }
    
    [Theory]
    [InlineData("parent2", Constants.Parent2Child1Id)]
    public async Task Test_AddGoal_WrongGoalType(string username, string childId)
    {
        await CheckForError(
            async () => await Client.PostPayloadAsync(Routes.Children.Goals.Add(childId),
                new GoalAddRequest(GoalType.Classroom,
                    GoalMetric.Completion,
                    "Title",
                    DateOnly.Parse("2025-01-01"),
                    DateOnly.Parse("2025-02-02"),
                    1),
                username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalTypeInvalid
        );
        
        await CheckForError(
            async () => await Client.PostPayloadAsync(Routes.Children.Goals.Add(childId),
                new GoalAddRequest(GoalType.ClassroomAggregate,
                    GoalMetric.Completion,
                    "Title",
                    DateOnly.Parse("2025-01-01"),
                    DateOnly.Parse("2025-02-02"),
                    1),
                username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalTypeInvalid
        );
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b2a4771d7f")]
    public async Task Test_AddGoal(string username, string childId)
    {
        const string completionGoalTitle = "CompletionGoal";
        const string booksGoalTitle = "NumBooksGoal";
        const int numBooksTarget = 5;
        DateOnly startDate = DateOnly.Parse("2024-12-01");
        DateOnly endDate = DateOnly.Parse("2025-01-01");
        
        GoalAddRequest requestCompletion = new(GoalType.Child, GoalMetric.Completion, completionGoalTitle, startDate, endDate, 0);
        string goalId1 = await CheckResponse<GoalResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.Children.Goals.Add(childId), requestCompletion, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(GoalMetric.Completion, content.GoalMetric);
                Assert.Equal(completionGoalTitle, content.Title);
                Assert.Equal(startDate, content.StartDate);
                Assert.Equal(endDate, content.EndDate);
                Assert.Equal(0, content.Target);
                return content.GoalId;
            });

        Goal? cg1 = await Context.Goals.FindAsync(goalId1);
        Assert.NotNull(cg1);
        Assert.Equal(GoalMetric.Completion, cg1.GoalMetric);
        Assert.Equal(completionGoalTitle, cg1.Title);
        Assert.Equal(startDate, cg1.StartDate);
        Assert.Equal(endDate, cg1.EndDate);
        Assert.Equal(0, cg1.Target);
        Assert.Single(this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals);
        
        GoalAddRequest requestNumBooks = new(GoalType.Child, GoalMetric.BooksRead, booksGoalTitle, startDate, endDate, numBooksTarget);
        string goalId2 = await CheckResponse<GoalResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.Children.Goals.Add(childId), requestNumBooks, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(GoalMetric.BooksRead, content.GoalMetric);
                Assert.Equal(booksGoalTitle, content.Title);
                Assert.Equal(startDate, content.StartDate);
                Assert.Equal(endDate, content.EndDate);
                Assert.Equal(numBooksTarget, content.Target);
                return content.GoalId;
            });

        Goal? cg2 = await Context.Goals.FindAsync(goalId2);
        Assert.NotNull(cg2);
        Assert.Equal(GoalMetric.BooksRead, cg2.GoalMetric);
        Assert.Equal(booksGoalTitle, cg2.Title);
        Assert.Equal(startDate, cg2.StartDate);
        Assert.Equal(endDate, cg2.EndDate);
        Assert.Equal(numBooksTarget, cg2.Target);
        Assert.Equal(2, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
    }
    
    
    // Edit Details
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", "NewComplete")]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", "NewNumBooks")]
    public async Task Test_EditGoalTitle(string username, string childId, string goalId, string newTitle)
    {
        int numChildGoals = this.Context.GoalChildren.Count(c => c.ChildId == childId);
        
        GoalEditRequest editRequest = new(Title: newTitle);
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(childId, goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(newTitle, newTitle);
            });

        Assert.Equal(numChildGoals, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
        GoalChild? g = await Context.GoalChildren.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(newTitle, g.Title);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", "2023-02-17")]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", "2013-10-04")]
    public async Task Test_EditGoalStartDate(string username, string childId, string goalId, string newStartDate)
    {
        int numChildGoals = this.Context.GoalChildren.Count(c => c.ChildId == childId);
        
        GoalEditRequest editRequest = new(StartDate: DateOnly.Parse(newStartDate));
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(childId, goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(DateOnly.Parse(newStartDate), content.StartDate);
            });

        Assert.Equal(numChildGoals, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
        GoalChild? g = await Context.GoalChildren.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(DateOnly.Parse(newStartDate), g.StartDate);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", "2023-02-17")]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", "2013-10-04")]
    public async Task Test_EditGoalEndDate(string username, string childId, string goalId, string newEndDate)
    {
        int numChildGoals = this.Context.GoalChildren.Count(c => c.ChildId == childId);
        
        GoalEditRequest editRequest = new(EndDate: DateOnly.Parse(newEndDate));
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(childId, goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(DateOnly.Parse(newEndDate), content.EndDate);
            });

        Assert.Equal(numChildGoals, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
        GoalChild? g = await Context.GoalChildren.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(DateOnly.Parse(newEndDate), g.EndDate);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", 5)]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", 7)]
    public async Task Test_EditGoalTargetNumBooks(string username, string childId, string goalId, int newTarget)
    {
        int numChildGoals = this.Context.GoalChildren.Count(c => c.ChildId == childId);
        
        GoalEditRequest editRequest = new(Target: newTarget);
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Children.Goals.Edit(childId, goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(newTarget, content.Target);
            });

        Assert.Equal(numChildGoals, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
        GoalChild? g = await Context.GoalChildren.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(newTarget, g.Target);
    }
    
    
    // Delete Goal
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193")]
    [InlineData("parent3", "2bc5b325697a55", "4aa48d2e0e7a77")]
    public async Task Test_DeleteGoal(string username, string childId, string goalId)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.Children.Goals.Delete(childId, goalId), username),
            HttpStatusCode.NoContent);
        
        Assert.Null(await Context.Goals.FindAsync(goalId));
    }
    
    
    // Goal Progress Logging Tests (child goals & class goals)
    
    [Theory]
    [InlineData(Constants.InvalidChildId, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_NotLoggedIn(string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 1)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("parent1", Constants.InvalidChildId, "InvalidGoalId")]
    [InlineData("parent2", Constants.Parent1Child1Id, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_ChildNotExistOrWrongParent(string username, string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 1),  username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_GoalNotExist(string username, string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 1),  username),
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("parent3", Constants.Parent1Child1Id, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_WrongChild(string username, string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 1),  username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }
    
    [Theory]
    [InlineData("parent2", "2bc5b21dcd7110", "4aa30b00417f9f")]
    public async Task Test_UpdateGoalLog_SameParentWrongChild(string username, string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 1),  username),
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "4aa30907023f3b")] // child goal
    [InlineData("parent1", Constants.Parent1Child1Id, "413a8732581806")] // class goal
    public async Task Test_UpdateGoalLog_NewCompletion(string username, string childId, string goalId)
    {
        await CheckResponse<bool>(
            async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 10_050), username),
            HttpStatusCode.OK,
            Assert.False);
        
        await CheckResponse<bool>(
            async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 25_100), username),
            HttpStatusCode.OK,
            Assert.True);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "4aa48941faee41")] // child goal
    [InlineData("parent1", Constants.Parent1Child1Id, "413b1d8ae65108")] // class goal
    public async Task Test_UpdateGoalLog_NewNumBooks(string username, string childId, string goalId)
    {
        await CheckResponse<bool>(
            async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 1), username),
            HttpStatusCode.OK,
            Assert.False);
        
        await CheckResponse<bool>(
            async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 2), username),
            HttpStatusCode.OK,
            Assert.True);
        
        await CheckResponse<bool>(
            async () => await Client.PutAsync(Routes.Children.Goals.Log(childId, goalId, 3), username),
            HttpStatusCode.OK,
            Assert.True);
    }
}