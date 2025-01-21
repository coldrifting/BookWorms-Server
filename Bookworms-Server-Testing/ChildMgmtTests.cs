using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ChildMgmtTests(BaseStartup<Program> factory) : BaseTest(factory)
{  
    [Theory]
    [InlineData("joey")]
    public async Task Test_AddChild_NotLoggedIn(string childName)
    {
        HttpResponseMessage response = await Client.PostAsync($"/user/children/add?childName={childName}", new StringContent(""));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Theory]
    [InlineData("teacher1", "joey")]
    [InlineData("admin", "joey")]
    public async Task Test_AddChild_NotAParent(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_AddChild_Basic(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "joey", "ash")]
    public async Task Test_AddChild_MultipleDistinct(string username, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName1}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName2}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/user/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Equal(2, children.Count);
        Assert.Contains(children, c => c.Name == childName1);
        Assert.Contains(children, c => c.Name == childName2);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_AddChild_MultipleSameName(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/user/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName);
    }
    
    [Theory]
    [InlineData("parent1", "parent4","joey")]
    public async Task Test_AddChild_MultipleSameName_DifferentParents(string parent1, string parent2, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), parent1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), parent2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/user/children/all", parent1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/user/children/all", parent2);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children2 = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.NotEmpty(children2);
        Assert.Single(children2);
        Assert.Contains(children2, c => c.Name == childName);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_RemoveChild_ChildNotExist(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/user/children/remove?childName={childName}", username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/user/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var children = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_RemoveChild_Basic(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.DeleteAsyncAsUser($"/user/children/remove?childName={childName}", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/user/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
    }
    
    [Theory]
    [InlineData("parent1", "joey", "alice")]
    public async Task Test_RemoveChild_DoesNotDeleteOtherChildren(string username, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName1}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName2}", new StringContent(""), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/user/children/remove?childName={childName1}", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/user/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName2);
    }
    
    [Theory]
    [InlineData("parent1", "parent3", "joey")]
    public async Task Test_RemoveChild_DoesNotDeleteSameChildNameUnderDifferentParent(string parent1, string parent2, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), parent1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), parent2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/user/children/remove?childName={childName}", parent1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/user/children/all", parent1);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
        
        HttpResponseMessage response5 = await Client.GetAsyncAsUser($"/user/children/all", parent2);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

        var children2 = await response5.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.NotEmpty(children2);
        Assert.Single(children2);
        Assert.Contains(children2, c => c.Name == childName);
    }
    
    [Theory]
    [InlineData("parent1", "Joey", "Andrew")]
    public async Task Test_EditChild_ChangeName_Basic(string parent, string childName, string newChildName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/edit?childName={childName}", 
            new ChildEditDTO(newChildName, null, null, null), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        // Check that we are not able to find child by the old name
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/user/children/edit?childName={childName}", 
            new ChildEditDTO(newChildName, null, null, null), parent);
        Assert.Equal(HttpStatusCode.NotFound, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/user/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.DoesNotContain(children, c => c.Name == childName);
        Assert.Contains(children, c => c.Name == newChildName);
    }
    
    [Theory]
    [InlineData("parent1", "Joey", "Andrew")]
    public async Task Test_EditChild_ChangeName_NameAlreadyExistsUnderParent(string parent, string childName, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}", new StringContent(""), parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName2}", new StringContent(""), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/user/children/edit?childName={childName}", 
            new ChildEditDTO(childName2, null, null, null), parent);
        Assert.Equal(HttpStatusCode.BadRequest, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/user/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Equal(2, children.Count);
        Assert.Contains(children, c => c.Name == childName);
        Assert.Contains(children, c => c.Name == childName2);
    }

    [Theory]
    [InlineData("parent1", "Joey", "A4")]
    public async Task Test_EditChild_ChangeReadingLevel_Basic(string parent, string childName, string readingLevel)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}",
            new StringContent(""), parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/edit?childName={childName}",
            new ChildEditDTO(null, readingLevel, null, null), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/user/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName);
        Assert.Contains(children, c => c.ReadingLevel == readingLevel);
    }

    [Theory]
    [InlineData("parent1", "Joey", "2017-08-31")]
    public async Task Test_EditChild_ChangeDOB_Basic(string parent, string childName, string dob)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/user/children/add?childName={childName}",
            new StringContent(""), parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/user/children/edit?childName={childName}",
            new ChildEditDTO(null, null, null, DateOnly.Parse(dob)), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/user/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName);
        Assert.Contains(children, c => c.DateOfBirth == DateOnly.Parse(dob));
    }
    
    // TODO - Add classroom code edit validation when classrooms are added
}