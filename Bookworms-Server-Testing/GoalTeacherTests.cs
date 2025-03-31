using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class GoalTeacherTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    private const string InvalidGoalId = "invalidGoalId";

    private const string ClassCompletionGoalId = "413b16cf615d35";
    private const string ClassBooksReadGoalId = "413b1d8ae564c2";
    
    private static readonly GoalAddRequest NewClassBooksReadGoal = new(
        GoalType.Classroom, 
        GoalMetric.BooksRead,
        "ClassCompletionGoalTitle", 
        DateOnly.Parse("2025-03-01"), 
        DateOnly.Parse("2025-04-02"), 
        2);
    
    private static readonly GoalAddRequest NewClassCompletionGoal = new(
        GoalType.Classroom, 
        GoalMetric.Completion,
        "ClassCompletionGoalTitle", 
        DateOnly.Parse("2025-05-01"), 
        DateOnly.Parse("2025-06-02"), 
        1);
    
    private static readonly GoalAddRequest NewClassAggregateMinutesRead = new(
        GoalType.ClassroomAggregate, 
        GoalMetric.MinutesRead,
        "ClassAggregateMinutesReadGoalTitle", 
        DateOnly.Parse("2025-07-01"), 
        DateOnly.Parse("2025-08-02"), 
        1);

    [Theory]
    [InlineData("admin")]
    [InlineData("parent1")]
    public async Task Test_TeacherGoalRoutes_NotTeacher(string username)
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.Classrooms.Goals.All(), username),
                async () => await Client.PostPayloadAsync(Routes.Classrooms.Goals.Add(), NewClassCompletionGoal, username),
                async () => await Client.PostPayloadAsync(Routes.Classrooms.Goals.Add(), NewClassAggregateMinutesRead, username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId: ClassCompletionGoalId), username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId: ClassBooksReadGoalId), username),
                async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(goalId: ClassCompletionGoalId), new GoalEditRequest(Title: "Title"), username),
                async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(goalId: ClassBooksReadGoalId), new GoalEditRequest(Title: "Title"), username),
                async () => await Client.DeleteAsync(Routes.Classrooms.Goals.Delete(goalId: ClassCompletionGoalId), username),
                async () => await Client.DeleteAsync(Routes.Classrooms.Goals.Delete(goalId: ClassBooksReadGoalId), username)
            ],
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
    }
    
    [Theory]
    [InlineData("teacher0")]
    public async Task Test_ClassGoalRoutes_NoClassCreated(string username)
    {
        await CheckForErrorBatch([
                async () => await Client.GetAsync(Routes.Classrooms.Goals.All(), username),
                async () => await Client.PostPayloadAsync(Routes.Classrooms.Goals.Add(), NewClassCompletionGoal, username),
                async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(InvalidGoalId), new GoalEditRequest(Target: 5), username),
                async () => await Client.DeleteAsync(Routes.Classrooms.Goals.Delete(InvalidGoalId), username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(InvalidGoalId), username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(InvalidGoalId, extended: true), username)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }
    
    [Theory]
    [InlineData("teacher3")]
    public async Task Test_ClassGoalRoutes_NoGoalsCreated(string username)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(InvalidGoalId), new GoalEditRequest(Target: 5), username),
                async () => await Client.DeleteAsync(Routes.Classrooms.Goals.Delete(InvalidGoalId), username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(InvalidGoalId), username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(InvalidGoalId, extended: true), username)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }
    
    [Theory]
    [InlineData("teacher2", "413b217f330ce8")]
    public async Task Test_ClassGoalRoutes_GoalFromOtherClass(string username, string goalId)
    {
        await CheckForErrorBatch([
                async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(goalId), new GoalEditRequest(Target: 5), username),
                async () => await Client.DeleteAsync(Routes.Classrooms.Goals.Delete(goalId), username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId), username),
                async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId, extended: true), username)
            ],
            HttpStatusCode.NotFound,
            ErrorResponse.GoalNotFound);
    }

    [Fact]
    public async Task Test_AllClassGoals_Basic()
    {
        await CheckResponse<List<GoalResponse>>(async () => await Client.GetAsync(Routes.Classrooms.Goals.All(), "teacher2"), HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(9, content.Count);
                
                foreach (GoalResponse v in content)
                {
                    Assert.NotNull(v.ClassGoalDetails);
                    Assert.Equal(2, v.ClassGoalDetails.StudentsTotal);
                }
                
                Assert.Contains(content, c => c is { GoalId: "413a8732533462", ClassGoalDetails.StudentsCompleted: 2, Progress: 37100});
                Assert.Contains(content, c => c is { GoalId: "413a8732545964", ClassGoalDetails.StudentsCompleted: 1, Progress: 37057});
                Assert.Contains(content, c => c is { GoalId: "413a8732584729", ClassGoalDetails.StudentsCompleted: 1, Progress: 12050});
                Assert.Contains(content, c => c is { GoalId: "413a8732581806", ClassGoalDetails.StudentsCompleted: 0, Progress: 0});
                Assert.Contains(content, c => c is { GoalId: "413a8732516249", ClassGoalDetails.StudentsCompleted: 0, Progress: 12025});

                Assert.Contains(content, c => c is { GoalId: "413b1d8ae564c2", ClassGoalDetails.StudentsCompleted: 2, Progress: 1, Target: 1});
                Assert.Contains(content, c => c is { GoalId: "413b1d8ae99498", ClassGoalDetails.StudentsCompleted: 1, Progress: 2, Target: 3});
                Assert.Contains(content, c => c is { GoalId: "413b1d8ae65108", ClassGoalDetails.StudentsCompleted: 1, Progress: 1, Target: 2});
                Assert.Contains(content, c => c is { GoalId: "413b1d8ae55090", ClassGoalDetails.StudentsCompleted: 0, Progress: 0, Target: 2});
            });
        
        await CheckResponse<List<GoalResponse>>(async () => await Client.GetAsync(Routes.Classrooms.Goals.All(), "teacher4"), HttpStatusCode.OK,
            content =>
            {
                Assert.Single(content);
                Assert.NotNull(content);
                Assert.Contains(content, c => c is { GoalId: "413b217f330ce8", ClassGoalDetails.StudentsTotal: 1, ClassGoalDetails.StudentsCompleted: 0, Target: 12, Progress: 0 });
            });
    }
    
    
    // Goal Details (Basic & All, all 9 test class goals)
    
    [Theory]
    [InlineData("teacher2", "413a8732533462", 2, 2, 37100)]
    [InlineData("teacher2", "413a8732545964", 2, 1, 37057)]
    [InlineData("teacher2", "413a8732584729", 2, 1, 12050)]
    [InlineData("teacher2", "413a8732581806", 2, 0, 0)]
    [InlineData("teacher2", "413a8732516249", 2, 0, 12025)]
    [InlineData("teacher2", "413b1d8ae564c2", 2, 2, 1, 1)]
    [InlineData("teacher2", "413b1d8ae99498", 2, 1, 2, 3)]
    [InlineData("teacher2", "413b1d8ae65108", 2, 1, 1, 2)]
    [InlineData("teacher2", "413b1d8ae55090", 2, 0, 0, 2)]
    public async Task Test_Details(string username, string goalId, int studentsTotal, int studentsCompleted, int progress, int? target = null)
    {
        await CheckResponse<GoalResponse>(
            async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.NotNull(content.ClassGoalDetails);
                Assert.Equal(studentsTotal, content.ClassGoalDetails.StudentsTotal);
                Assert.Equal(studentsCompleted, content.ClassGoalDetails.StudentsCompleted);
                Assert.Equal(progress, content.Progress);
                if (target is not null)
                {
                    Assert.Equal(target, content.Target);
                }
                Assert.Null(content.ClassGoalDetails.StudentGoalDetails);
            });
        
        await CheckResponse<GoalResponse>(
            async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId, extended: true), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.NotNull(content.ClassGoalDetails);
                Assert.Equal(studentsTotal, content.ClassGoalDetails.StudentsTotal);
                Assert.Equal(studentsCompleted, content.ClassGoalDetails.StudentsCompleted);
                Assert.Equal(progress, content.Progress);
                if (target is not null)
                {
                    Assert.Equal(target, content.Target);
                }

                Assert.NotNull(content.ClassGoalDetails.StudentGoalDetails);
                Assert.Equal(studentsTotal, content.ClassGoalDetails.StudentGoalDetails.Count);
                Assert.Equal(studentsCompleted,
                    content.GoalMetric == GoalMetric.Completion
                        ? content.ClassGoalDetails.StudentGoalDetails.Count(s => s.Progress % 1000 >= 100)
                        : content.ClassGoalDetails.StudentGoalDetails.Count(s => s.Progress >= content.Target));
                foreach (StudentGoalDetails s in content.ClassGoalDetails.StudentGoalDetails)
                {
                    Assert.NotNull(s.Name);
                }
            });
    }
    
    
    // New Goal
    
    [Theory]
    [InlineData("teacher1")]
    public async Task Test_AllGoals_OneGoalNoStudents(string username)
    {
        await CheckResponse<List<GoalResponse>>(
            async () => await Client.GetAsync(Routes.Classrooms.Goals.All(), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.NotNull(content);
                Assert.Single(content);
            });
    }
    
    [Theory]
    [InlineData("teacher2")]
    public async Task Test_AddGoal_ChildGoal_Invalid(string username)
    {
        await CheckForError(
                async () => await Client.PostPayloadAsync(Routes.Classrooms.Goals.Add(), new GoalAddRequest(
                    GoalType.Child, 
                    GoalMetric.Completion, 
                    "", 
                    DateOnly.Parse("2025-01-01"), 
                    DateOnly.Parse("2025-02-02"), 
                    5), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorResponse.GoalTypeInvalid);
    }
    
    [Theory]
    [InlineData("teacher2")]
    public async Task Test_AddGoal(string username)
    {
        string goalId1 = await CheckResponse<GoalResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.Classrooms.Goals.Add(), NewClassCompletionGoal, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(NewClassCompletionGoal.Title, content.Title);
                return content.GoalId;
            });

        Goal? gc = await Context.Goals.FindAsync(goalId1);
        Assert.NotNull(gc);
        Assert.Equal(NewClassCompletionGoal.Title, gc.Title);
        Assert.Equal(10, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        
        string goalId2 = await CheckResponse<GoalResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.Classrooms.Goals.Add(), NewClassBooksReadGoal, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(NewClassBooksReadGoal.Title, content.Title);
                return content.GoalId;
            });

        Assert.Equal(11, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        Goal? gn = await Context.Goals.FindAsync(goalId2);
        Assert.NotNull(gn);
        Assert.Equal(NewClassBooksReadGoal.Title, gn.Title);
    }

    
    // Edit Details
    
    [Theory]
    [InlineData("teacher2", "413a8732516249", "NewComplete")]
    [InlineData("teacher2", "413b1d8ae55090", "NewNumBooks")]
    public async Task Test_EditGoalTitle(string username, string goalId, string newTitle)
    {
        GoalEditRequest editRequest = new(Title: newTitle);
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(newTitle, content.Title);
            });

        Assert.Equal(9, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        Goal? g = await Context.Goals.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(newTitle, g.Title);
    }
    
    [Theory]
    [InlineData("teacher2", "413a8732516249", "2023-02-17")]
    [InlineData("teacher2", "413b1d8ae55090", "2013-10-04")]
    public async Task Test_EditGoalStartDate(string username, string goalId, string newStartDate)
    {
        GoalEditRequest editRequest = new(StartDate: DateOnly.Parse(newStartDate));
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(DateOnly.Parse(newStartDate), content.StartDate);
            });

        Assert.Equal(9, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        Goal? g = await Context.Goals.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(DateOnly.Parse(newStartDate), g.StartDate);
    }
    
    [Theory]
    [InlineData("teacher2", "413a8732516249", "2023-02-17")]
    [InlineData("teacher2", "413b1d8ae55090", "2013-10-04")]
    public async Task Test_EditGoalEndDate(string username, string goalId, string newEndDate)
    {
        GoalEditRequest editRequest = new(EndDate: DateOnly.Parse(newEndDate));
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(DateOnly.Parse(newEndDate), content.EndDate);
            });

        Assert.Equal(9, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        Goal? g = await Context.Goals.FindAsync(goalId);
        Assert.NotNull(g);
        Assert.Equal(DateOnly.Parse(newEndDate), g.EndDate);
    }
    
    [Theory]
    [InlineData("teacher2", "413b1d8ae55090", 7)]
    [InlineData("teacher2", "413b1d8ae65108", 5)]
    public async Task Test_EditGoalTarget(string username, string goalId, int newTarget)
    {
        GoalEditRequest editRequest = new(Target: newTarget);
        
        await CheckResponse<GoalResponse>(
            async () => await Client.PutPayloadAsync(Routes.Classrooms.Goals.Edit(goalId), editRequest, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(goalId, content.GoalId);
                Assert.Equal(newTarget, content.Target);
            });

        Assert.Equal(9, Context.Classrooms.Include(classroom => classroom.Goals).First(c => c.TeacherUsername == username).Goals.Count);
        Goal? g = await Context.Goals.FindAsync(goalId);
        Assert.NotNull(g);
    }
    
    // Delete Goal
    
    [Theory]
    [InlineData("teacher2", "413a8732516249")]
    [InlineData("teacher2", "413b1d8ae55090")]
    public async Task Test_DeleteGoal(string username, string goalId)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.Classrooms.Goals.Delete(goalId), username),
            HttpStatusCode.NoContent);
        
        Assert.Null(await Context.Goals.FindAsync(goalId));
    }
    
    [Theory]
    [InlineData("teacher6", "ZZYZX2")]
    public async Task Test_ClassWideGoal(string username, string classCode)
    {
        string goalId = await CheckResponse<GoalResponse, string>(
            async () => await Client.PostPayloadAsync(Routes.Classrooms.Goals.Add(), new GoalAddRequest(
                GoalType.ClassroomAggregate,
                GoalMetric.MinutesRead,
                "GoalAggregateMinutesRead",
                 DateOnly.Parse("2024-06-02"),
                DateOnly.Parse("2025-02-12"),
                50), username),
            HttpStatusCode.OK,
            content => content.GoalId);

        
        await Client.PostAsync(Routes.Classrooms.Join(Constants.Parent1Child1Id, classCode), "parent1");
        await Client.PutAsync(Routes.Children.Goals.Log(Constants.Parent1Child1Id, goalId, 10), "parent1");

        await CheckResponse<GoalResponse>(
            async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId, true), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(GoalType.ClassroomAggregate, content.GoalType);
                Assert.Equal(GoalMetric.MinutesRead, content.GoalMetric);
                Assert.Equal(10, content.Progress);
            });
        
        await Client.PostAsync(Routes.Classrooms.Join(Constants.Parent2Child1Id, classCode), "parent2");
        await Client.PutAsync(Routes.Children.Goals.Log(Constants.Parent2Child1Id, goalId, 30), "parent2");
        
        await CheckResponse<GoalResponse>(
            async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId, true), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(GoalType.ClassroomAggregate, content.GoalType);
                Assert.Equal(GoalMetric.MinutesRead, content.GoalMetric);
                Assert.Equal(40, content.Progress);
            });
        
        await Client.PutAsync(Routes.Children.Goals.Log(Constants.Parent1Child1Id, goalId, 15), "parent1");
        
        await CheckResponse<GoalResponse>(
            async () => await Client.GetAsync(Routes.Classrooms.Goals.Details(goalId, true), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(GoalType.ClassroomAggregate, content.GoalType);
                Assert.Equal(GoalMetric.MinutesRead, content.GoalMetric);
                Assert.Equal(45, content.Progress);
            });
    }
}