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
    private static readonly ChildGoalAddRequest ChildGoalAddCompletionRequest = new ("Completion Goal", DateOnly.Parse("2025-05-01"));
    private static readonly ChildGoalEditRequest ChildGoalEditCompletionRequest = new ("Edit Goal Completion", DateOnly.Parse("2025-05-01"), null);
    
    [Fact]
    public async Task Test_ChildGoalRoutes_NotLoggedIn()
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.ChildGoals.All(SomeChildId)),
                async () => await Client.PostPayloadAsync(Routes.ChildGoals.Add(SomeChildId), new {}),
                async () => await Client.PutPayloadAsync(Routes.ChildGoals.Edit(SomeChildId, SomeGoalId), new {}),
                async () => await Client.DeleteAsync(Routes.ChildGoals.Delete(SomeChildId, SomeGoalId)),
                async () => await Client.GetAsync(Routes.ChildGoals.Details(SomeChildId, SomeGoalId))
            ],
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("admin")]
    [InlineData("teacher0")]
    public async Task Test_ChildGoalRoutes_NotParent(string parentUsername)
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.ChildGoals.All(SomeChildId), parentUsername),
                async () => await Client.PostPayloadAsync(Routes.ChildGoals.Add(SomeChildId), ChildGoalAddCompletionRequest, parentUsername),
                async () => await Client.PutPayloadAsync(Routes.ChildGoals.Edit(SomeChildId, SomeGoalId), ChildGoalEditCompletionRequest, parentUsername),
                async () => await Client.DeleteAsync(Routes.ChildGoals.Delete(SomeChildId, SomeGoalId), parentUsername),
                async () => await Client.GetAsync(Routes.ChildGoals.Details(SomeChildId, SomeGoalId), parentUsername)
            ],
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b2a4771d7f")]
    public async Task Test_ChildGoalRoutes_NoGoalsCreated(string parentUsername, string childId)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.ChildGoals.Edit(childId, SomeGoalId), new {}, parentUsername),
                async () => await Client.DeleteAsync(Routes.ChildGoals.Delete(childId, SomeGoalId), parentUsername),
                async () => await Client.GetAsync(Routes.ChildGoals.Details(childId, SomeGoalId), parentUsername)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa310dc9dd437")]
    public async Task Test_ChildGoalRoutes_GoalFromOtherChild(string parentUsername, string childId, string goalId)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.ChildGoals.Edit(childId, goalId), new {}, parentUsername),
                async () => await Client.DeleteAsync(Routes.ChildGoals.Delete(childId, goalId), parentUsername),
                async () => await Client.GetAsync(Routes.ChildGoals.Details(childId, goalId), parentUsername),
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }

    [Theory]
    [InlineData("parent1", "2bc5ae6239c988", "parent3", "2bc5b325697a55")]
    public async Task Test_AllChildGoals_Basic(string parentUsername1, string childId1, string parentUsername2, string childId2)
    {
        await CheckResponse<AllGoalOverviewResponse>(async () => await Client.GetAsync(Routes.ChildGoals.All(childId1), parentUsername1), HttpStatusCode.OK,
            content =>
            {
                Assert.Single(content.CompletionGoals);
                Assert.Single(content.NumBookGoals);
                Assert.Equal(5, content.ClassCompletionGoals.Count);
                Assert.Equal(4, content.ClassNumBooksGoals.Count);
                
                Assert.Contains(content.CompletionGoals, c => c is { GoalId: "4aa30907023f3b", Progress: 0.0f, Duration: 0 });
                Assert.Contains(content.NumBookGoals, c => c is { GoalId: "4aa48941faee41", TargetNumBooks: 2, NumBooks: 0 });
                
                Assert.Contains(content.ClassCompletionGoals, c => c is { GoalId: "413a8732533462", Progress: 1.0f, Duration: 25 });
                Assert.Contains(content.ClassCompletionGoals, c => c is { GoalId: "413a8732545964", Progress: 0.15f, Duration: 25 });
                Assert.Contains(content.ClassCompletionGoals, c => c is { GoalId: "413a8732584729", Progress: 0.0f, Duration: 0 });
                Assert.Contains(content.ClassCompletionGoals, c => c is { GoalId: "413a8732516249", Progress: 0.0f, Duration: 0 });
                Assert.Contains(content.ClassCompletionGoals, c => c is { GoalId: "413a8732581806", Progress: 0.0f, Duration: 0 });
                
                Assert.Contains(content.ClassNumBooksGoals, c => c is { GoalId: "413b1d8ae564c2", TargetNumBooks: 1, NumBooks: 2 });
                Assert.Contains(content.ClassNumBooksGoals, c => c is { GoalId: "413b1d8ae99498", TargetNumBooks: 3, NumBooks: 3 });
                Assert.Contains(content.ClassNumBooksGoals, c => c is { GoalId: "413b1d8ae65108", TargetNumBooks: 2, NumBooks: 0 });
                Assert.Contains(content.ClassNumBooksGoals, c => c is { GoalId: "413b1d8ae55090", TargetNumBooks: 2, NumBooks: 0 });
            });
        
        await CheckResponse<AllGoalOverviewResponse>(async () => await Client.GetAsync(Routes.ChildGoals.All(childId2), parentUsername2), HttpStatusCode.OK,
            content =>
            {
                Assert.Single(content.CompletionGoals);
                Assert.Single(content.NumBookGoals);
                Assert.Empty(content.ClassCompletionGoals);
                Assert.Empty(content.ClassNumBooksGoals);
                
                Assert.Contains(content.CompletionGoals, c => c is { GoalId: "4aa30cf4a23193", Progress: 0.5f, Duration: 2 });
                Assert.Contains(content.NumBookGoals, c => c is { GoalId: "4aa48d2e0e7a77", TargetNumBooks: 2, NumBooks: 2 });
            });
    }
    
    
    // Goal Details (all 12 test child goals)
    
    [Theory]
    [InlineData("parent1", "2bc5ae6239c988", "4aa30907023f3b", 0.0f, 0, null, null)]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa30b00417f9f", 0.0f, 5, null, null)]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", 0.5f, 2, null, null)]
    [InlineData("parent4", "2bc5b4544b34e7", "4aa30ee8b6d4e3", 0.5f, 20, null, null)]
    [InlineData("parent3", "2bc5b3b701b2ee", "4aa310dc9dd437", 1.0f, 2, null, null)]
    [InlineData("parent5", "2bc5b4ec4f452c", "4aa312d0e19e10", 1.0f, 20, null, null)]
    [InlineData("parent1", "2bc5ae6239c988", "4aa48941faee41", null, null, 2, 0)]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", null, null, 2, 1)]
    [InlineData("parent3", "2bc5b325697a55", "4aa48d2e0e7a77", null, null, 2, 2)]
    [InlineData("parent4", "2bc5b4544b34e7", "4aa48f227d01fd", null, null, 0, 0)]
    [InlineData("parent3", "2bc5b3b701b2ee", "4aa49116545ede", null, null, 10, 10)]
    [InlineData("parent5", "2bc5b4ec4f452c", "4aa4930a9675ae", null, null, 20, 20)]
    public async Task Test_Details(string username, string childId, string goalId, float? progress, int? duration, int? numBooks, int? targetNumBooks )
    {
        await CheckResponse<GenericGoalChildResponse>(
            async () => await Client.GetAsync(Routes.ChildGoals.Details(childId, goalId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                switch (content)
                {
                    case ChildGoalCompletionResponse contentCompletion:
                        Assert.Equal(progress, contentCompletion.Progress);
                        Assert.Equal(duration, contentCompletion.Duration);
                        break;
                    case ChildGoalNumBooksResponse contentNumBooks:
                        Assert.Equal(targetNumBooks, contentNumBooks.TargetNumBooks);
                        Assert.Equal(numBooks, contentNumBooks.NumBooks);
                        break;
                }
            });
    }
    
    
    // New Goal
    
    [Theory]
    [InlineData("parent3", "2bc5b2a4771d7f")]
    public async Task Test_AddGoal(string parentUsername, string childId)
    {
        ChildGoalAddRequest requestCompletion = new("CompletionGoal", DateOnly.Parse("2025-01-01"));
        var goalId1 = await CheckResponse<ChildGoalCompletionResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.ChildGoals.Add(childId), requestCompletion, parentUsername),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal("CompletionGoal", content.Title);
                Assert.Equal(DateOnly.Parse("2025-01-01"), content.EndDate);
                return content.GoalId;
            });

        var cg1 = await Context.ChildGoals.FindAsync(goalId1);
        Assert.NotNull(cg1);
        Assert.Equal("CompletionGoal", cg1.Title);
        Assert.Single(this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals);
        
        ChildGoalAddRequest requestNumBooks = new("NumBooksGoal", DateOnly.Parse("2025-02-02"), 5);
        var goalId2 = await CheckResponse<ChildGoalNumBooksResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.ChildGoals.Add(childId), requestNumBooks, parentUsername),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal("NumBooksGoal", content.Title);
                Assert.Equal(DateOnly.Parse("2025-02-02"), content.EndDate);
                Assert.Equal(5, content.TargetNumBooks);
                return content.GoalId;
            });

        var cg2 = await Context.ChildGoals.FindAsync(goalId2);
        Assert.NotNull(cg2);
        Assert.Equal("NumBooksGoal", cg2.Title);
        Assert.Equal(2, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
    }
    
    
    // Edit Details
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", "NewComplete")]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", "NewNumBooks")]
    public async Task Test_EditGoalTitle(string parentUsername, string childId, string goalId, string newTitle)
    {
        int numChildGoals = this.Context.ChildGoals.Count(c => c.ChildId == childId);
        
        ChildGoalEditRequest editRequest = new(newTitle, null, null);
        
        await CheckResponse<GenericGoalChildResponse>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.Edit(childId, goalId), editRequest, parentUsername),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(newTitle, content.Title);
            });

        Assert.Equal(numChildGoals, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
        ChildGoal? g = await Context.ChildGoals.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(newTitle, g.Title);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", "2023-02-17")]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", "2013-10-04")]
    public async Task Test_EditGoalEndDate(string parentUsername, string childId, string goalId, string newEndDate)
    {
        int numChildGoals = this.Context.ChildGoals.Count(c => c.ChildId == childId);
        
        ChildGoalEditRequest editRequest = new(null, DateOnly.Parse(newEndDate), null);
        
        await CheckResponse<GenericGoalChildResponse>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.Edit(childId, goalId), editRequest, parentUsername),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(DateOnly.Parse(newEndDate), content.EndDate);
            });

        Assert.Equal(numChildGoals, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
        ChildGoal? g = await Context.ChildGoals.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(DateOnly.Parse(newEndDate), g.EndDate);
    }
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193", 5)]
    [InlineData("parent2", "2bc5b121b46b4a", "4aa48b3a078e81", 7)]
    public async Task Test_EditGoalTargetNumBooks(string username, string childId, string goalId, int targetNumBooks)
    {
        int numChildGoals = this.Context.ChildGoals.Count(c => c.ChildId == childId);
        
        ChildGoalEditRequest editRequest = new(null, null, targetNumBooks);
        
        await CheckResponse<GenericGoalChildResponse>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.Edit(childId, goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                if (content is ChildGoalNumBooksResponse numBooksContent)
                {
                    Assert.Equal(targetNumBooks, numBooksContent.TargetNumBooks);
                }
            });

        Assert.Equal(numChildGoals, this.Context.Children.Include(child => child.Goals).First(c => c.ChildId == childId).Goals.Count);
        ChildGoal? g = await Context.ChildGoals.FindAsync(goalId);
        Assert.NotNull(g);

        // Should succeed but do nothing if goal type is completion metric
        if (g is ChildGoalNumBooks nbg)
        {
            Assert.Equal(targetNumBooks, nbg.TargetNumBooks);
        }
    }
    
    
    // Delete Goal
    
    [Theory]
    [InlineData("parent3", "2bc5b325697a55", "4aa30cf4a23193")]
    [InlineData("parent3", "2bc5b325697a55", "4aa48d2e0e7a77")]
    public async Task Test_DeleteGoal(string parentUsername, string childId, string goalId)
    {
        int numClassGoals = this.Context.Children
            .Include(child => child.Classrooms)
            .ThenInclude(classroom => classroom.Goals)
            .FirstOrDefault(child => child.ChildId == childId)!
            .Classrooms.SelectMany(c => c.Goals).Count();
        
        await CheckResponse<AllGoalOverviewResponse>(
            async () => await Client.DeleteAsync(Routes.ChildGoals.Delete(childId, goalId), parentUsername),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(1, content.CompletionGoals.Count + content.NumBookGoals.Count);
                Assert.DoesNotContain(content.CompletionGoals, c => c.GoalId == goalId);
                Assert.DoesNotContain(content.NumBookGoals, c => c.GoalId == goalId);
                Assert.Equal(numClassGoals, content.ClassCompletionGoals.Count + content.ClassNumBooksGoals.Count);
            });
        
        Assert.Null(await Context.ChildGoals.FindAsync(goalId));
    }
    
    
    // Goal Progress Logging Tests (child goals & class goals)
    
    [Theory]
    [InlineData(Constants.InvalidChildId, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_NotLoggedIn(string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutAsync(Routes.ChildGoals.UpdateProgress(childId, goalId)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("admin", Constants.InvalidChildId, "InvalidGoalId")]
    [InlineData("teacher1", Constants.InvalidChildId, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_NotParent(string username, string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), new {}, username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent1", Constants.InvalidChildId, "InvalidGoalId")]
    [InlineData("parent2", Constants.Parent1Child1Id, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_ChildNotExistOrWrongParent(string username, string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), new {}, username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "InvalidGoalId")]
    public async Task Test_UpdateGoalLog_GoalNotExist(string username, string childId, string goalId)
    {
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), new {},  username),
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "4aa30907023f3b")] // child goal
    [InlineData("parent1", Constants.Parent1Child1Id, "413a8732581806")] // class goal
    public async Task Test_UpdateGoalLog_NewCompletion(string username, string childId, string goalId)
    {
        ClassGoalLogEditRequest request = new(NumBooks: 5);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Duration: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0, Duration: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Progress: 0, Duration: 1);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Progress: 1, Duration: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Progress: 1);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Duration: 10);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0.5f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.OK,
            Assert.False);
        
        request = new(Progress: 1f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.OK,
            Assert.True);
        
        // Should ignore extra fields
        request = new(NumBooks: 5, Progress: 1f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.OK,
            Assert.True);
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "4aa48941faee41")] // child goal
    [InlineData("parent1", Constants.Parent1Child1Id, "413b1d8ae65108")] // class goal
    public async Task Test_UpdateGoalLog_NewNumBooks(string username, string childId, string goalId)
    {
        GoalProgressUpdateRequest request = new(Progress: 1);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(NumBooks: 0);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoInvalid);
        
        request = new(Duration: 10);
        await CheckForError(async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(Progress: 0.5f, Duration: 10);
        await CheckForError(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.BadRequest,
            ErrorResponse.GoalEditInfoMissing);
        
        request = new(NumBooks: 1);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.OK,
            Assert.False);
        
        request = new(NumBooks: 2);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.OK,
            Assert.True);
        
        request = new(NumBooks: 3);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.OK,
            Assert.True);
        
        // Should ignore extra fields
        request = new(NumBooks: 1, Progress: 1f, Duration: 10);
        await CheckResponse<bool>(
            async () => await Client.PutPayloadAsync(Routes.ChildGoals.UpdateProgress(childId, goalId), request, username),
            HttpStatusCode.OK,
            Assert.False);
    }
}