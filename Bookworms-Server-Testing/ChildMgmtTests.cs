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
        
        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.UserNotParent, content);
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
        
        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.Unauthorized, content);
    }
    
    [Theory]
    [InlineData("teacher1", "joey")]
    [InlineData("admin", "joey")]
    public async Task Test_AddChild_NotAParent(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.UserNotParent, content);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_AddChild_Basic(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var children1 = await response.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
    }
    
    [Theory]
    [InlineData("parent1", "joey", "ash")]
    public async Task Test_AddChild_MultipleDistinct(string username, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName1}/add", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName1 && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/add", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Equal(2, children2.Count);
        Assert.Contains(children2, c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children2, c => c.Name == childName2 && c.Selected != true);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Equal(2, children3.Count);
        Assert.Contains(children3, c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children3, c => c.Name == childName2 && c.Selected != true);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_AddChild_MultipleSameName(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);
        
        ErrorDTO? content2 = await response2.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content2);
        Assert.Equal(ErrorDTO.ChildAlreadyExists, content2);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3, c => c.Name == childName && c.Selected == true);
    }
    
    [Theory]
    [InlineData("parent1", "parent4","joey")]
    public async Task Test_AddChild_MultipleSameName_DifferentParents(string parent1, string parent2, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Single(children2);
        Assert.Contains(children2, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent2);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children4 = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children4);
        Assert.Single(children4);
        Assert.Contains(children4, c => c.Name == childName && c.Selected == true);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_RemoveChild_ChildNotExist(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);
        
        ErrorDTO? content1 = await response1.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.ChildNotFound, content1);
        
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
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", username);
        Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);
        
        ErrorDTO? content1 = await response1.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.UserNotParent, content1);
    }
    
    [Theory]
    [InlineData("parent1", "joey")]
    public async Task Test_RemoveChild_Basic(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Empty(children2);
        
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
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName1 && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/add", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Equal(2, children2.Count);
        Assert.Contains(children2, c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children2, c => c.Name == childName2 && c.Selected != true);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/children/{childName1}/remove", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3, c => c.Name == childName2 && c.Selected == true);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.Name == childName2);
    }
    
    [Theory]
    [InlineData("parent1", "parent3", "joey")]
    public async Task Test_RemoveChild_DoesNotDeleteSameChildNameUnderDifferentParent(string parent1, string parent2, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Single(children2);
        Assert.Contains(children2, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/children/{childName}/remove", parent1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Empty(children3);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent1);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children4 = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children4);
        Assert.Empty(children4);
        
        HttpResponseMessage response5 = await Client.GetAsyncAsUser($"/children/all", parent2);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

        var children5 = await response5.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children5);
        Assert.Single(children5);
        Assert.Contains(children5, c => c.Name == childName && c.Selected == true);
    }
    
    [Theory]
    [InlineData("teacher1", "childName")]
    public async Task Test_EditChild_NotParent(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.UserNotParent, content1);
    }
    
    [Theory]
    [InlineData("parent1", "Joey", "Andrew")]
    public async Task Test_EditChild_ChangeName_Basic(string parent, string childName, string newChildName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", 
            new ChildEditDTO(newChildName), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(newChildName, child2.Name);
        Assert.True(child2.Selected);
        
        // Check that we are not able to find child by the old name
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", 
            new ChildEditDTO(newChildName), parent);
        Assert.Equal(HttpStatusCode.NotFound, response3.StatusCode);
        
        ErrorDTO? content3 = await response3.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content3);
        Assert.Equal(ErrorDTO.ChildNotFound, content3);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Single(children);
        Assert.DoesNotContain(children, c => c.Name == childName);
        Assert.Contains(children, c => c.Name == newChildName && c.Selected == true);
    }

    [Theory]
    [InlineData("parent1", "Joey", "IconInvalid")]
    public async Task Test_EditChild_ChangeIcon_InvalidIconIndex(string parent, string childName, string newIcon)
    {
        await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new { }, parent);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit",
            new ChildEditDTO(ChildIcon: newIcon), parent);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);

        ErrorDTO? error = await response2.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(error);
        Assert.Equal(ErrorDTO.InvalidIconIndex, error);
    }

    [Theory]
    [InlineData("parent1", "Joey", "Icon3")]
    public async Task Test_EditChild_ChangeIcon_Basic(string parent, string childName, string newIcon)
    {
        await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new { }, parent);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit",
            new ChildEditDTO(ChildIcon: newIcon), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(newIcon, child2.ChildIcon);
    }

    [Theory]
    [InlineData("parent1", "Joey", "Andrew")]
    public async Task Test_EditChild_ChangeName_NameAlreadyExistsUnderParent(string parent, string childName, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Equal(2, children2.Count);
        Assert.Contains(children2, c => c.Name == childName && c.Selected == true);
        Assert.Contains(children2, c => c.Name == childName2 && c.Selected != true);
        
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit", 
            new ChildEditDTO(childName2), parent);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response3.StatusCode);
        
        ErrorDTO? content3 = await response3.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content3);
        Assert.Equal(ErrorDTO.ChildAlreadyExists, content3);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children4 = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children4);
        Assert.Equal(2, children4.Count);
        Assert.Contains(children4, c => c.Name == childName && c.Selected == true);
        Assert.Contains(children4, c => c.Name == childName2 && c.Selected != true);
    }

    [Theory]
    [InlineData("parent1", "Joey", "A4")]
    public async Task Test_EditChild_ChangeReadingLevel_Basic(string parent, string childName, string readingLevel)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add",
            new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit",
            new ChildEditDTO(ReadingLevel: readingLevel), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(childName, child2.Name);
        Assert.Equal(readingLevel, child2.ReadingLevel);
        Assert.True(child2.Selected);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3, c => c.Name == childName && c.Selected == true && c.ReadingLevel == readingLevel);
    }

    [Theory]
    [InlineData("parent1", "Joey", "2017-08-31")]
    public async Task Test_EditChild_ChangeDOB_Basic(string parent, string childName, string dob)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add",
            new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);

        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/edit",
            new ChildEditDTO(DateOfBirth: DateOnly.Parse(dob)), parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(childName, child2.Name);
        Assert.Equal(DateOnly.Parse(dob), child2.DateOfBirth);
        Assert.True(child2.Selected);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3,
            c => c.Name == childName && c.Selected == true && c.DateOfBirth == DateOnly.Parse(dob));
    }


    [Theory]
    [InlineData("Joey")]
    public async Task Test_SelectChild_NotLoggedIn(string child)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsync($"/children/{child}/select", new {});
        Assert.Equal(HttpStatusCode.Unauthorized, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.Unauthorized, content1);
        
        HttpResponseMessage response2 = await Client.GetAsync($"/children/selected");
        Assert.Equal(HttpStatusCode.Unauthorized, response2.StatusCode);

        ErrorDTO? content2 = await response2.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content2);
        Assert.Equal(ErrorDTO.Unauthorized, content2);
    }
    
    [Theory]
    [InlineData("teacher1", "Joey")]
    public async Task Test_SelectChild_NotParent(string username, string child)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/select", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.UserNotParent, content1);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.Forbidden, response2.StatusCode);
        
        ErrorDTO? content2 = await response2.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content2);
        Assert.Equal(ErrorDTO.UserNotParent, content2);
    }
    
    [Theory]
    [InlineData("parent1", "Joey")]
    public async Task Test_SelectChild_ChildNotExist(string username, string child)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/select", new {}, username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.ChildNotFound, content1);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
    }

    [Theory]
    [InlineData("parent1", "Joey")]
    public async Task Test_SelectChild_Basic(string parent, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1,
            c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(childName, child2.Name);
        Assert.True(child2.Selected);
        
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/children/{childName}/select", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        ChildResponseDTO? child3 = await response3.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child3);
        Assert.Equal(childName, child3.Name);
        Assert.True(child3.Selected);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/selected", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        ChildResponseDTO? selectedChild = await response4.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(selectedChild);
        Assert.Equal(childName, selectedChild.Name);
        Assert.True(selectedChild.Selected);
        
        HttpResponseMessage response5 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);
        
        var children5 = await response5.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children5);
        Assert.Single(children5);
        Assert.Contains(children5, c => c.Name == childName && c.Selected == true);
    }

    [Theory]
    [InlineData("parent1", "Joey", "Ashley")]
    public async Task Test_SelectChild_Multiple(string parent, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{childName1}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1,
            c => c.Name == childName1 && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Equal(2, children2.Count);
        Assert.Contains(children2,
            c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children2,
            c => c.Name == childName2 && c.Selected != true);
        
        HttpResponseMessage response3 = await Client.PostAsJsonAsyncAsUser($"/children/{childName1}/select", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        ChildResponseDTO? child3 = await response3.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child3);
        Assert.Equal(childName1, child3.Name);
        Assert.True(child3.Selected);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
        
        var children4 = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children4);
        Assert.Equal(2, children4.Count);
        Assert.Contains(children4, c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children4, c => c.Name == childName2 && c.Selected != true);
        
        HttpResponseMessage response5 = await Client.PostAsJsonAsyncAsUser($"/children/{childName2}/select", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

        ChildResponseDTO? child5 = await response5.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child5);
        Assert.Equal(childName2, child5.Name);
        Assert.True(child5.Selected);
        
        HttpResponseMessage response6 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response6.StatusCode);
        
        var children6 = await response6.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children6);
        Assert.Equal(2, children6.Count);
        Assert.Contains(children6, c => c.Name == childName1 && c.Selected != true);
        Assert.Contains(children6, c => c.Name == childName2 && c.Selected == true);
    }
    
    // TODO - Add more classroom code edit validation when classrooms are added

    [Theory]
    [InlineData("parent1", "Joey", "BadVal")]
    public async Task Test_EditChild_InvalidClassroomCode(string parent, string child, string classroomCode)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/add", new {}, parent);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1,
            c => c.Name == child && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/{child}/edit", new ChildEditDTO(ClassroomCode: classroomCode, ReadingLevel: "A5"), parent);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);

        ErrorDTO? content2 = await response2.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content2);
        Assert.Equal(ErrorDTO.ClassroomNotFound, content2);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", parent);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3,
            c => c.Name == child && c.ReadingLevel is null && c.ClassroomCode is null && c.Selected == true);
    }
}