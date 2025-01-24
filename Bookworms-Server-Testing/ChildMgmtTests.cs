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
    [InlineData("parent0")]
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
        HttpResponseMessage response = await Client.PostAsync($"/children/add?childName={childName}", new StringContent(""));
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
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName}", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        
        ErrorDTO? content = await response.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content);
        Assert.Equal(ErrorDTO.UserNotParent, content);
    }
    
    [Theory]
    [InlineData("parent0", "Jason")]
    public async Task Test_AddChild_Basic(string username, string childName)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName}", new {}, username);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        string? childGuid = response.Headers.Location?.ToString().Replace("/children/", "");
        Assert.NotNull(childGuid);
        Assert.NotEmpty(childGuid);
        
        var children1 = await response.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true && c.ChildId.ToString() == childGuid);
    }
    
    [Theory]
    [InlineData("parent0", "joey", "ash")]
    [InlineData("parent0", "joey", "joey")]
    public async Task Test_AddChild_Multiple(string username, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName1}", new {}, username);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        
        string? child1Guid = response1.Headers.Location?.ToString().Replace("/children/", "");
        Assert.NotNull(child1Guid);
        Assert.NotEmpty(child1Guid);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName1 && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName2}", new {}, username);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        
        string? child2Guid = response2.Headers.Location?.ToString().Replace("/children/", "");
        Assert.NotNull(child2Guid);
        Assert.NotEmpty(child2Guid);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Equal(2, children2.Count);
        Assert.Contains(children2, c => c.Name == childName1 && c.Selected == true && c.ChildId.ToString() == child1Guid);
        Assert.Contains(children2, c => c.Name == childName2 && c.Selected != true && c.ChildId.ToString() == child2Guid);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Equal(2, children3.Count);
        Assert.Contains(children3, c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children3, c => c.Name == childName2 && c.Selected != true);
    }
    
    [Theory]
    [InlineData("parent0", "parent5","joey")]
    public async Task Test_AddChild_MultipleSameName_DifferentParents(string username1, string username2, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName}", new {}, username1);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName}", new {}, username2);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Single(children2);
        Assert.Contains(children2, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3, c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", username2);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var children4 = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children4);
        Assert.Single(children4);
        Assert.Contains(children4, c => c.Name == childName && c.Selected == true);
    }
    
    [Theory]
    [InlineData("parent1", "389b78f0-13f1-4003-8be4-9a72cb145d9e", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
    public async Task Test_RemoveChild_ChildNotExist(string username, Guid invalidChildId, Guid existingChildId)
    {
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/children/{invalidChildId}/remove", username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);
        
        ErrorDTO? content1 = await response1.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.ChildNotFound, content1);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var children = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children);
        Assert.Single(children);
        Assert.Contains(children, c => c.ChildId == existingChildId && c.Selected == true);
    }
    
    [Theory]
    [InlineData("teacher1", "389b78f0-13f1-4003-8be4-9a72cb145d9e")]
    public async Task Test_RemoveChild_NotParent(string username, Guid childId)
    {
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/children/{childId}/remove", username);
        Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);
        
        ErrorDTO? content1 = await response1.Content.ReadFromJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.UserNotParent, content1);
    }
    
    [Theory]
    [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
    public async Task Test_RemoveChild_Basic(string username, Guid childId)
    {
        HttpResponseMessage response2 = await Client.DeleteAsyncAsUser($"/children/{childId}/remove", username);
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
    [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "08dd3c4b-f197-4657-8556-58c76701802b")]
    public async Task Test_RemoveChild_DoesNotDeleteOtherChildren(string username, Guid childToRemoveId, Guid childLeftId)
    {
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/children/{childToRemoveId}/remove", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1, c => c.ChildId == childLeftId && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Single(children2);
        Assert.Contains(children2, c => c.ChildId == childLeftId && c.Selected == true);
    }
    
    [Theory]
    [InlineData("teacher1", "e1740f4a-9855-472d-bf47-fb57dce6c1b2")]
    public async Task Test_EditChild_NotParent(string username, Guid childId)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit", new {}, username);
        Assert.Equal(HttpStatusCode.Forbidden, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.UserNotParent, content1);
    }
    
    [Theory]
    [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Rachel")]
    public async Task Test_EditChild_ChangeName_Basic(string username, Guid childId, string newName)
    {
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit", 
            new ChildEditDTO(newName), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(newName, child2.Name);
        Assert.True(child2.Selected);
        
        // Check that we are not able to find child by the old name
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var content3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Single(content3);
        Assert.Contains(content3, c => c.ChildId == childId && c.Name == newName && c.Selected == true);
    }

    [Theory]
    [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "IconInvalid")]
    public async Task Test_EditChild_ChangeIcon_InvalidIconIndex(string username, Guid childId, string newIcon)
    {
        HttpResponseMessage response1 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var content1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content1);
        Assert.Single(content1);
        
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit",
            new ChildEditDTO(ChildIcon: newIcon), username);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);

        ErrorDTO? error = await response2.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(error);
        Assert.Equal(ErrorDTO.InvalidIconIndex, error);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var content3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Single(content3);
        Assert.Equal(content1, content3);
    }

    [Theory]
    [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Icon3")]
    public async Task Test_EditChild_ChangeIcon_Basic(string username, Guid childId, string newIcon)
    {
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit",
            new ChildEditDTO(ChildIcon: newIcon), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(newIcon, child2.ChildIcon);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var content3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Equal(3, content3.Count);
        Assert.Contains(content3, c => c.ChildId == childId && c.ChildIcon == newIcon);
    }

    [Theory]
    [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f", "Costanza")]
    public async Task Test_EditChild_ChangeName_NameAlreadyExistsUnderParent(string username, Guid childId, Guid childId2, string newName)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit", new ChildEditDTO(NewName: newName), username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var child1 = await response1.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child1);
        Assert.Equal(childId, child1.ChildId);
        Assert.Equal(newName, child1.Name);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Equal(3, children2.Count);
        Assert.Contains(children2, c => c.Name == newName && c.ChildId == childId);
        Assert.Contains(children2, c => c.Name == newName && c.ChildId == childId2);
    }

    [Theory]
    [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "A4")]
    public async Task Test_EditChild_ChangeReadingLevel_Basic(string username, Guid childId, string readingLevel)
    {
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit",
            new ChildEditDTO(ReadingLevel: readingLevel), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(childId, child2.ChildId);
        Assert.Equal(readingLevel, child2.ReadingLevel);
        Assert.True(child2.Selected);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Single(children3);
        Assert.Contains(children3, c => c.ChildId == childId && c.Selected == true && c.ReadingLevel == readingLevel);
    }

    [Theory]
    [InlineData("parent3", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f", "1960-08-31")]
    public async Task Test_EditChild_ChangeDOB_Basic(string username, Guid childId, string dob)
    {
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit",
            new ChildEditDTO(DateOfBirth: DateOnly.Parse(dob)), username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(childId, child2.ChildId);
        Assert.Equal(DateOnly.Parse(dob), child2.DateOfBirth);

        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Equal(3, children3.Count);
        Assert.Contains(children3,
            c => c.ChildId == childId && c.DateOfBirth == DateOnly.Parse(dob));
    }
    
    [Theory]
    [InlineData("61a1be57-69a3-46d5-95cd-8e257d4a553c")]
    public async Task Test_SelectChild_NotLoggedIn(Guid childId)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsync($"/children/{childId}/select", new {});
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
    [InlineData("teacher1", "61a1be57-69a3-46d5-95cd-8e257d4a553c")]
    public async Task Test_SelectChild_NotParent(string username, Guid childId)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/select", new {}, username);
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
    [InlineData("parent5", "61a1be57-69a3-46d5-95cd-8e257d4a553c")]
    public async Task Test_SelectChild_WrongParent(string username, Guid childId)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/select", new {}, username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.ChildNotFound, content1);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
    }
    
    [Theory]
    [InlineData("parent1", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
    public async Task Test_SelectChild_WrongParent_PrevSelection(string username, Guid childId, Guid correctChildId)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/select", new {}, username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.ChildNotFound, content1);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var content2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(content2);
        Assert.Equal(correctChildId, content2.ChildId);
    }
    
    [Theory]
    [InlineData("parent5", "8f28044c-8dc6-4e4c-9e59-971e0d618910")]
    public async Task Test_SelectChild_ChildNotExist(string username, Guid childId)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/select", new {}, username);
        Assert.Equal(HttpStatusCode.NotFound, response1.StatusCode);

        ErrorDTO? content1 = await response1.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content1);
        Assert.Equal(ErrorDTO.ChildNotFound, content1);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.NoContent, response2.StatusCode);
    }
    
    [Theory]
    [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20")]
    public async Task Test_SelectChild_SameName(string username, Guid childId)
    {
        HttpResponseMessage response1 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var content1 = await response1.Content.ReadFromJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content1);
        Assert.Equal(2, content1.Count);
        Assert.True(content1[0].Selected != true ? content1[1].Selected : content1[0].Selected);

        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var content2 = await response2.Content.ReadFromJsonAsync<ChildResponseDTO>();
        Assert.NotNull(content2);
        Assert.NotEqual(childId, content2.ChildId);
        
        HttpResponseMessage response3 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/select", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadFromJsonAsync<ChildResponseDTO>();
        Assert.NotNull(content3);
        Assert.Equal(childId, content3.ChildId);
        Assert.True(content3.Selected);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var content4 = await response4.Content.ReadFromJsonAsync<ChildResponseDTO>();
        Assert.NotNull(content4);
        Assert.True(content4.Selected);
        
        HttpResponseMessage response5 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);
        
        var content5 = await response5.Content.ReadFromJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content5);
        Assert.Equal(2, content5.Count);
        Assert.Contains(content5, c => c.ChildId == childId && c.Selected == true);
        Assert.Contains(content5, c => c.ChildId != childId && c.Selected != true);
    }
    
    [Theory]
    [InlineData("parent2", "08dd3c4b-f197-4657-8556-58c76701802b", "c5dca20d-849d-418f-b65b-cb79aa723c20")]
    [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "08dd3c4b-f197-4657-8556-58c76701802b")]
    public async Task Test_SelectChild_RemoveSelection(string username, Guid childId1, Guid childId2)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/children/{childId2}/select", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var content1 = await response1.Content.ReadFromJsonAsync<ChildResponseDTO>();
        Assert.NotNull(content1);
        Assert.Equal(childId2, content1.ChildId);
        Assert.True(content1.Selected);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var content2 = await response2.Content.ReadFromJsonAsync<ChildResponseDTO>();
        Assert.NotNull(content2);
        Assert.Equal(childId2, content2.ChildId);
        Assert.True(content2.Selected);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/children/{childId2}/remove", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadFromJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Single(content3);
        Assert.DoesNotContain(content3, c => c.ChildId == childId2);
        Assert.Contains(content3, c => c.ChildId == childId1 && c.Selected == true);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
        
        var content4 = await response4.Content.ReadFromJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(content4);
        Assert.Single(content4);
        Assert.DoesNotContain(content4, c => c.ChildId == childId2);
        Assert.Contains(content4, c => c.ChildId == childId1 && c.Selected == true);
    }

    [Theory]
    [InlineData("parent5", "Kaitlin")]
    public async Task Test_ChildSelectedAfterCreated(string username, string childName)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName}", new {}, username);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        
        string? child1Guid = response1.Headers.Location?.ToString().Replace("/children/", "");
        Assert.NotNull(child1Guid);
        Assert.NotEmpty(child1Guid);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1,
            c => c.Name == childName && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        ChildResponseDTO? child2 = await response2.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child2);
        Assert.Equal(childName, child2.Name);
        Assert.Equal(child1Guid, child2.ChildId.ToString());
        Assert.True(child2.Selected);
    }

    [Theory]
    [InlineData("parent0", "Joey", "Ashley")]
    public async Task Test_ChildSelectedAfterCreated_Multiple(string username, string childName1, string childName2)
    {
        HttpResponseMessage response1 = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName1}", new {}, username);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
        
        string? child1Guid = response1.Headers.Location?.ToString().Replace("/children/", "");
        Assert.NotNull(child1Guid);
        Assert.NotEmpty(child1Guid);
        
        var children1 = await response1.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children1);
        Assert.Single(children1);
        Assert.Contains(children1,
            c => c.Name == childName1 && c.Selected == true);
        
        HttpResponseMessage response2 = await Client.PostAsJsonAsyncAsUser($"/children/add?childName={childName2}", new {}, username);
        Assert.Equal(HttpStatusCode.Created, response2.StatusCode);
        
        string? child2Guid = response2.Headers.Location?.ToString().Replace("/children/", "");
        Assert.NotNull(child2Guid);
        Assert.NotEmpty(child2Guid);
        
        var children2 = await response2.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children2);
        Assert.Equal(2, children2.Count);
        Assert.Contains(children2,
            c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children2,
            c => c.Name == childName2 && c.Selected != true);
        
        HttpResponseMessage response3 = await Client.PutAsJsonAsyncAsUser($"/children/{child1Guid}/select", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        ChildResponseDTO? child3 = await response3.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child3);
        Assert.Equal(childName1, child3.Name);
        Assert.True(child3.Selected);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
        
        var children4 = await response4.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children4);
        Assert.Equal(2, children4.Count);
        Assert.Contains(children4, c => c.Name == childName1 && c.Selected == true);
        Assert.Contains(children4, c => c.Name == childName2 && c.Selected != true);
        
        HttpResponseMessage response5 = await Client.PutAsJsonAsyncAsUser($"/children/{child2Guid}/select", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

        ChildResponseDTO? child5 = await response5.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(child5);
        Assert.Equal(childName2, child5.Name);
        Assert.True(child5.Selected);
        
        HttpResponseMessage response6 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response6.StatusCode);
        
        var children6 = await response6.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children6);
        Assert.Equal(2, children6.Count);
        Assert.Contains(children6, c => c.Name == childName1 && c.Selected != true);
        Assert.Contains(children6, c => c.Name == childName2 && c.Selected == true);
        
        HttpResponseMessage response7 = await Client.GetAsyncAsUser($"/children/selected", username);
        Assert.Equal(HttpStatusCode.OK, response7.StatusCode);
        
        var children7 = await response7.Content.ReadJsonAsync<ChildResponseDTO>();
        Assert.NotNull(children7);
        
        HttpResponseMessage response8 = await Client.DeleteAsyncAsUser($"/children/{children7.ChildId}/remove", username);
        Assert.Equal(HttpStatusCode.OK, response8.StatusCode);
        
        var children8 = await response8.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children8);
        Assert.Single(children8);
        Assert.Contains(children8, c => c.Name == childName1 && c.Selected == true);
    }
    
    // TODO - Add more classroom code edit validation when classrooms are added

    [Theory]
    [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "BadVal")]
    public async Task Test_EditChild_InvalidClassroomCode(string username, Guid childId, string classroomCode)
    {
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/children/{childId}/edit", 
            new ChildEditDTO(ClassroomCode: classroomCode, ReadingLevel: "A5"), username);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response2.StatusCode);

        ErrorDTO? content2 = await response2.Content.ReadJsonAsync<ErrorDTO>();
        Assert.NotNull(content2);
        Assert.Equal(ErrorDTO.ClassroomNotFound, content2);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/children/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        
        var children3 = await response3.Content.ReadJsonAsync<List<ChildResponseDTO>>();
        Assert.NotNull(children3);
        Assert.Equal(2, children3.Count);
        Assert.Contains(children3,
            c => c.ChildId == childId && c.ClassroomCode is null);
    }
}