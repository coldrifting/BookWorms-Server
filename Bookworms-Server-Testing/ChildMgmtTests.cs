using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ChildMgmtTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    [Theory]
    [InlineData("teacher1")]
    public async Task Test_GetAllChildren_NotParent(string username)
    {
        HttpResponseMessage response = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1")]
    public async Task Test_GetAllChildren_NoChildren(string username)
    {
        HttpResponseMessage response = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var children = await response.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
    }
    
    [Theory]
    [InlineData("joey")]
    public async Task Test_AddChild_NotLoggedIn(string childName)
    {
        HttpResponseMessage response = await Client.PostAsync($"/children/{childName}/add", new StringContent(""));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Theory]
    [InlineData("teacher1", "joey")]
    [InlineData("admin", "joey")]
    public async Task Test_AddChild_NotAParent(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_AddChild_Basic(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "joey", "ash")]
    public async Task Test_AddChild_MultipleDistinct(string username, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName1}/add", new {}, username);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/add", new {}, username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
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
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
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
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent1);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent2);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent2);
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
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var children = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
    }
    
    [Theory]
    [InlineData("teacher1", "childName")]
    public async Task Test_RemoveChild_NotParent(string username, string childName)
    {
        HttpResponseMessage response = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", username);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_RemoveChild_Basic(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
    }
    
    [Theory]
    [InlineData("parent1", "joey", "alice")]
    public async Task Test_RemoveChild_DoesNotDeleteOtherChildren(string username, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName1}/add", new {}, username);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/add", new {}, username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/children/{childName1}/remove", username);
        Assert.Equal(HttpStatusCode.NoContent, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

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
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent1);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent2);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", parent1);
        Assert.Equal(HttpStatusCode.NoContent, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent1);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Empty(children);
        
        HttpResponseMessage response5 = await Client.GetAsyncAsUser($"/children/all", parent2);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

        var children2 = await response5.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.NotEmpty(children2);
        Assert.Single(children2);
        Assert.Contains(children2, c => c.Name == childName);
    }
    
    [Theory]
    [InlineData("teacher1", "childName")]
    public async Task Test_EditChild_NotParent(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "Joey", "Andrew")]
    public async Task Test_EditChild_ChangeName_Basic(string parent, string childName, string newChildName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", 
            new ChildEditDTO(newChildName), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        // Check that we are not able to find child by the old name
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", 
            new ChildEditDTO(newChildName, null, null, null), parent);
        Assert.Equal(HttpStatusCode.NotFound, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent);
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
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", 
            new ChildEditDTO(childName2, null, null, null), parent);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent);
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
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add",
            new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit",
            new ChildEditDTO(null, readingLevel, null, null), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent);
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
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add",
            new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit",
            new ChildEditDTO(null, null, null, DateOnly.Parse(dob)), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName);
        Assert.Contains(children, c => c.DateOfBirth == DateOnly.Parse(dob));
    }


    [Theory]
    [InlineData("Joey")]
    public async Task Test_SelectChild_NotLoggedIn(string child)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsync($"/children/{child}/select", new {});
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.GetAsync($"/children/selected");
        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);
    }
    
    [Theory]
    [InlineData("teacher1", "Joey")]
    public async Task Test_SelectChild_NotParent(string username, string child)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/select", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "Joey")]
    public async Task Test_SelectChild_ChildNotExist(string username, string child)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/select", new {}, username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
    }

    [Theory]
    [InlineData("parent1", "Joey")]
    public async Task Test_SelectChild_Basic(string parent, string child)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response0 = await Client.GetAsyncAsUser($"/children/selected", parent);
        Assert.Equal(HttpStatusCode.NoContent, response0.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/select", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/selected", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        ChildResponseDTO? selectedChild = await response3.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(selectedChild);
        Assert.Equal(child, selectedChild.Name);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
        
        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == child && c.Selected == true);
    }

    [Theory]
    [InlineData("parent1", "Joey", "Ashley")]
    public async Task Test_SelectChild_Multiple(string parent, string child1, string child2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child1}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{child2}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/children/{child1}/select", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
        
        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.NotEmpty(children);
        Assert.Equal(2, children.Count);
        Assert.Contains(children, c => c.Name == child1 && c.Selected == true);
        Assert.Contains(children, c => c.Name == child2 && c.Selected != true);
        
        HttpResponseMessage response5 = await Client.PostAsJsonAsyncAsUser($"/children/{child2}/select", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response5.StatusCode);
        
        HttpResponseMessage response6 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response6.StatusCode);
        
        var children2 = await response6.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.NotEmpty(children2);
        Assert.Equal(2, children2.Count);
        Assert.Contains(children2, c => c.Name == child1 && c.Selected != true);
        Assert.Contains(children2, c => c.Name == child2 && c.Selected == true);
    }
    
    // TODO - Add more classroom code edit validation when classrooms are added

    [Theory]
    [InlineData("parent1", "Joey", "BadVal")]
    public async Task Test_EditChild_InvalidClassroomCode(string parent, string child, string classroomCode)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.NoContent, response1.StatusCode);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/edit", new ChildEditDTO(classroomCode: classroomCode, readingLevel: "A5"), parent);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);

        ErrorDTO? content = await response2.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.ClassroomNotFound, content);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var children = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == child && c.ReadingLevel is null && c.ClassroomCode is null);
    }
}