using System.Net;
using System.Net.Http.Json;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

public abstract class ChildMgmtTests
{
    [Collection("Integration Tests")]
    public class ChildMgmtReadOnlyTests(AppFactory<Program> factory) : BaseTestReadOnlyFixture(factory)
    {
        [Theory]
        [InlineData("teacher1")]
        public async Task Test_GetAllChildren_NotParent(string username)
        {
            await CheckForError(() => Client.GetAsync($"/children/all", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0")]
        public async Task Test_GetAllChildren_NoChildren(string username)
        {
            HttpResponseMessage response = await Client.GetAsync($"/children/all", username);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var children = await response.Content.ReadJsonAsync<List<ChildResponseDTO>>();
            Assert.NotNull(children);
            Assert.Empty(children);
        }

        [Theory]
        [InlineData("joey")]
        public async Task Test_AddChild_NotLoggedIn(string childName)
        {
            await CheckForError(() => Client.PostAsync($"/children/add?childName={childName}"),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "joey")]
        [InlineData("admin", "joey")]
        public async Task Test_AddChild_NotAParent(string username, string childName)
        {
            await CheckForError(() => Client.PostAsync($"/children/add?childName={childName}", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent1", "389b78f0-13f1-4003-8be4-9a72cb145d9e", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
        public async Task Test_RemoveChild_ChildNotExist(string username, Guid invalidChildId, Guid existingChildId)
        {
            await CheckForError(() => Client.DeleteAsync($"/children/{invalidChildId}/remove", username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == existingChildId && c.Selected == true);
                });
        }

        [Theory]
        [InlineData("teacher1", "389b78f0-13f1-4003-8be4-9a72cb145d9e")]
        public async Task Test_RemoveChild_NotParent(string username, Guid childId)
        {
            await CheckForError(() => Client.DeleteAsync($"/children/{childId}/remove", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("teacher1", "e1740f4a-9855-472d-bf47-fb57dce6c1b2")]
        public async Task Test_EditChild_NotParent(string username, Guid childId)
        {
            await CheckForError(
                () => Client.PutPayloadAsync($"/children/{childId}/edit", new ChildEditDTO(), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("61a1be57-69a3-46d5-95cd-8e257d4a553c")]
        public async Task Test_SelectChild_NotLoggedIn(Guid childId)
        {
            await CheckForError(() => Client.PutAsync($"/children/{childId}/select"),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);

            await CheckForError(() => Client.GetAsync($"/children/selected"),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "61a1be57-69a3-46d5-95cd-8e257d4a553c")]
        public async Task Test_SelectChild_NotParent(string username, Guid childId)
        {
            await CheckForError(() => Client.PutAsync($"/children/{childId}/select", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);

            await CheckForError(() => Client.GetAsync($"/children/selected", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "61a1be57-69a3-46d5-95cd-8e257d4a553c")]
        public async Task Test_SelectChild_WrongParent_NoOtherChildren(string username, Guid childId)
        {
            await CheckForError(() => Client.PutAsync($"/children/{childId}/select", username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
            
            await CheckResponse(async () => await Client.GetAsync($"/children/selected", username), HttpStatusCode.NoContent);
        }

        [Theory]
        [InlineData("parent1", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
        public async Task Test_SelectChild_WrongParent_PrevSelection(string username, Guid childId, Guid correctChildId)
        {
            await CheckForError(() => Client.PutAsync($"/children/{childId}/select", username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
            
            await CheckResponse<ChildResponseDTO>(async () => await Client.GetAsync($"/children/selected", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(correctChildId, content.ChildId);
                });
        }
        
        [Theory]
        [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "BadVal")]
        public async Task Test_EditChild_InvalidClassroomCode(string username, Guid childId, string classroomCode)
        {
            await CheckForError(() => Client.PutPayloadAsync($"/children/{childId}/edit",
                    new ChildEditDTO(ClassroomCode: classroomCode, ReadingLevel: "A5"), username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.ClassroomNotFound);
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId == childId && c.ClassroomCode is null && c.ReadingLevel == null);
                });
        }

        [Theory]
        [InlineData("parent2", "c5dca20d-1234-418f-b65b-cb79aa723c20", "newName")]
        public async Task Test_EditChild_InvalidGuid(string username, Guid childId, string newName)
        {
            await CheckForError(() => Client.PutPayloadAsync($"/children/{childId}/edit",
                    new ChildEditDTO(NewName: newName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("parent0", "8f28044c-8dc6-4e4c-9e59-971e0d618910")]
        public async Task Test_SelectChild_ChildNotExist_NoOtherChildren(string username, Guid childId)
        {
            await CheckForError(() => Client.PutAsync($"/children/{childId}/select", username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
            
            await CheckResponse(async () => await Client.GetAsync($"/children/selected", username), HttpStatusCode.NoContent);
        }
    }
    
    [Collection("Integration Tests")]
    public class ChildMgmtWriteTests(AppFactory<Program> factory) : BaseTestWriteFixture(factory)
    {
        [Theory]
        [InlineData("parent0", "Jason")]
        public async Task Test_AddChild_First(string username, string childName)
        {
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.PostAsync($"/children/add?childName={childName}", username),
                HttpStatusCode.Created,
                (content, headers) => {
                    string? childGuid = headers.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(childGuid);
                    Assert.NotEmpty(childGuid);
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId.ToString() == childGuid && c.Selected == true);
                });
        }
        
        [Theory]
        [InlineData("parent1", "Jason")]
        public async Task Test_AddChild_Second(string username, string childName)
        {
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.PostAsync($"/children/add?childName={childName}", username),
                HttpStatusCode.Created,
                (content, headers) => {
                    string? childGuid = headers.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(childGuid);
                    Assert.NotEmpty(childGuid);
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId.ToString() == childGuid && c.Selected != true);
                });
        }

        [Theory]
        [InlineData("parent0", "joey", "ash")]
        [InlineData("parent0", "joey", "joey")]
        public async Task Test_AddChild_MultipleSameParent(string username, string childName1, string childName2)
        {
            string childGuid1 = await CheckResponse<List<ChildResponseDTO>, string>(async () => await Client.PostAsync($"/children/add?childName={childName1}", username),
                HttpStatusCode.Created,
                (content, headers) => {
                    string? childGuid = headers.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(childGuid);
                    Assert.NotEmpty(childGuid);
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId.ToString() == childGuid && c.Name == childName1 && c.Selected == true);
                    return childGuid;
                });
            
            string childGuid2 = await CheckResponse<List<ChildResponseDTO>, string>(async () => await Client.PostAsync($"/children/add?childName={childName2}", username),
                HttpStatusCode.Created,
                (content, headers) => {
                    string? childGuid = headers.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(childGuid);
                    Assert.NotEmpty(childGuid);
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId.ToString() == childGuid && c.Name == childName2 && c.Selected != true);
                    Assert.Contains(content, c => c.ChildId.ToString() == childGuid1 && c.Name == childName1 && c.Selected == true);
                    return childGuid;
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId.ToString() == childGuid1 && c.Name == childName1 && c.Selected == true);
                    Assert.Contains(content, c => c.ChildId.ToString() == childGuid2 && c.Name == childName2 && c.Selected != true);
                });
        }

        [Theory]
        [InlineData("parent0", "parent5", "joey")]
        public async Task Test_AddChild_MultipleSameName_DifferentParents(string username1, string username2,
            string childName)
        {
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.PostAsync($"/children/add?childName={childName}", username1),
                HttpStatusCode.Created,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName && c.Selected == true);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.PostAsync($"/children/add?childName={childName}", username2),
                HttpStatusCode.Created,
                (content) => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name != childName && c.Selected == true);
                    Assert.Contains(content, c => c.Name == childName && c.Selected != true);
                });

            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username1),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName && c.Selected == true);
                });

            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username2),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name != childName && c.Selected == true);
                    Assert.Contains(content, c => c.Name == childName && c.Selected != true);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
        public async Task Test_RemoveChild_Basic(string username, Guid childId)
        {
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.DeleteAsync($"/children/{childId}/remove", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Empty(content);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Empty(content);
                });
        }

        [Theory]
        [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "08dd3c4b-f197-4657-8556-58c76701802b")]
        public async Task Test_RemoveChild_DoesNotDeleteOtherChildren(string username, Guid childToRemoveId, Guid childLeftId)
        {
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.DeleteAsync($"/children/{childToRemoveId}/remove", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childLeftId && c.Selected == true);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childLeftId && c.Selected == true);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Rachel")]
        public async Task Test_EditChild_ChangeName_Basic(string username, Guid childId, string newName)
        {
            await CheckResponse<ChildResponseDTO>(async () => await Client.PutPayloadAsync($"/children/{childId}/edit", 
                    new ChildEditDTO(newName), username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(newName, content.Name);
                    Assert.True(content.Selected);
                });

            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childId && c.Name == newName && c.Selected == true);
                });
        }

        [Theory]
        [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "IconInvalid")]
        public async Task Test_EditChild_ChangeIcon_InvalidIconIndex(string username, Guid childId, string newIcon)
        {
            await CheckForError(() => Client.PutPayloadAsync($"/children/{childId}/edit",
                    new ChildEditDTO(ChildIcon: newIcon), username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.InvalidIconIndex);
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childId && c.ChildIcon == "Icon1");
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Icon3")]
        public async Task Test_EditChild_ChangeIcon_Basic(string username, Guid childId, string newIcon)
        {
            await CheckResponse<ChildResponseDTO>(async () => await Client.PutPayloadAsync($"/children/{childId}/edit",
                new ChildEditDTO(ChildIcon: newIcon), username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(newIcon, content.ChildIcon);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(3, content.Count);
                    Assert.Contains(content, c => c.ChildId == childId && c.ChildIcon == newIcon);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f",
            "Costanza")]
        public async Task Test_EditChild_ChangeName_NameAlreadyExistsUnderParent(string username, Guid childId, Guid childId2, string newName)
        {
            await CheckResponse<ChildResponseDTO>(async () => await Client.PutPayloadAsync($"/children/{childId}/edit",
                new ChildEditDTO(NewName: newName), username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(newName, content.Name);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(3, content.Count);
                    Assert.Contains(content, c => c.Name == newName && c.ChildId == childId);
                    Assert.Contains(content, c => c.Name == newName && c.ChildId == childId2);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "A4")]
        public async Task Test_EditChild_ChangeReadingLevel_Basic(string username, Guid childId, string readingLevel)
        {
            await CheckResponse<ChildResponseDTO>(async () => await Client.PutPayloadAsync($"/children/{childId}/edit",
                new ChildEditDTO(ReadingLevel: readingLevel), username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(readingLevel, content.ReadingLevel);
                    Assert.True(content.Selected);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childId && c.Selected == true && c.ReadingLevel == readingLevel);
                });
        }

        [Theory]
        [InlineData("parent3", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f", "1960-08-31")]
        public async Task Test_EditChild_ChangeDOB_Basic(string username, Guid childId, string dob)
        {
            await CheckResponse<ChildResponseDTO>(async () => await Client.PutPayloadAsync($"/children/{childId}/edit",
                new ChildEditDTO(DateOfBirth: DateOnly.Parse(dob)), username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(DateOnly.Parse(dob), content.DateOfBirth);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(3, content.Count);
                    Assert.Contains(content, c => c.ChildId == childId && c.DateOfBirth == DateOnly.Parse(dob));
                });
        }

        [Theory]
        [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20")]
        public async Task Test_SelectChild_SameName(string username, Guid childId)
        {
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(2, content.Count);
                    Assert.True(content[0].Selected != true ? content[1].Selected : content[0].Selected);
                });
            
            await CheckResponse<ChildResponseDTO>(async () => await Client.GetAsync($"/children/selected", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.NotEqual(childId, content.ChildId);
                });
            
            await CheckResponse<ChildResponseDTO>(async () => await Client.PutAsync($"/children/{childId}/select", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.True(content.Selected);
                });
            
            await CheckResponse<ChildResponseDTO>(async () => await Client.GetAsync($"/children/selected", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.True(content.Selected);
                });

            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId == childId && c.Selected == true);
                    Assert.Contains(content, c => c.ChildId != childId && c.Selected != true);
                });
        }

        [Theory]
        [InlineData("parent2", "08dd3c4b-f197-4657-8556-58c76701802b", "c5dca20d-849d-418f-b65b-cb79aa723c20")]
        [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "08dd3c4b-f197-4657-8556-58c76701802b")]
        public async Task Test_SelectChild_RemoveSelection(string username, Guid childId1, Guid childId2)
        {
            await CheckResponse<ChildResponseDTO>(async () => await Client.PutAsync($"/children/{childId2}/select", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childId2, content.ChildId);
                    Assert.True(content.Selected);
                });
            
            await CheckResponse<ChildResponseDTO>(async () => await Client.GetAsync($"/children/selected", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childId2, content.ChildId);
                    Assert.True(content.Selected);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.DeleteAsync($"/children/{childId2}/remove", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.DoesNotContain(content, c => c.ChildId == childId2);
                    Assert.Contains(content, c => c.ChildId == childId1 && c.Selected == true);
                });

            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.DoesNotContain(content, c => c.ChildId == childId2);
                    Assert.Contains(content, c => c.ChildId == childId1 && c.Selected == true);
                });
        }

        [Theory]
        [InlineData("parent0", "Kaitlin")]
        public async Task Test_ChildSelectedAfterCreated_NoOtherChildren(string username, string childName)
        {
            string child1Guid = await CheckResponse<List<ChildResponseDTO>, string>(async () => await Client.PostAsync($"/children/add?childName={childName}", username),
                HttpStatusCode.Created,
                (content, header) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName && c.Selected == true);
                    
                    string? child1Guid = header.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(child1Guid);
                    Assert.NotEmpty(child1Guid);
                    return child1Guid;
                });
            
            await CheckResponse<ChildResponseDTO>(async () => await Client.GetAsync($"/children/selected", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childName, content.Name);
                    Assert.Equal(child1Guid, content.ChildId.ToString());
                    Assert.True(content.Selected);
                });
        }

        [Theory]
        [InlineData("parent0", "Joey", "Ashley")]
        public async Task Test_ChildSelectedAfterCreated_Multiple(string username, string childName1, string childName2)
        {
            string child1Guid = await CheckResponse<List<ChildResponseDTO>, string>(async () => await Client.PostAsync($"/children/add?childName={childName1}", username),
                HttpStatusCode.Created,
                (content, header) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName1 && c.Selected == true);
                    
                    string? child1Guid = header.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(child1Guid);
                    Assert.NotEmpty(child1Guid);
                    return child1Guid;
                });
            
            string child2Guid = await CheckResponse<List<ChildResponseDTO>, string>(async () => await Client.PostAsync($"/children/add?childName={childName2}", username),
                HttpStatusCode.Created,
                (content, header) => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name == childName1 && c.Selected == true);
                    Assert.Contains(content, c => c.Name == childName2 && c.Selected != true);
                    
                    string? child2Guid = header.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(child2Guid);
                    Assert.NotEmpty(child2Guid);
                    return child2Guid;
                });

            await CheckResponse<ChildResponseDTO>(async () => await Client.PutAsync($"/children/{child1Guid}/select", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childName1, content.Name);
                    Assert.Equal(child1Guid, content.ChildId.ToString());
                    Assert.True(content.Selected);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name == childName1 && c.Selected == true);
                    Assert.Contains(content, c => c.Name == childName2 && c.Selected != true);
                });

            await CheckResponse<ChildResponseDTO>(async () => await Client.PutAsync($"/children/{child2Guid}/select", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childName2, content.Name);
                    Assert.Equal(child2Guid, content.ChildId.ToString());
                    Assert.True(content.Selected);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name == childName1 && c.Selected != true);
                    Assert.Contains(content, c => c.Name == childName2 && c.Selected == true);
                });
            
            await CheckResponse<ChildResponseDTO>(async () => await Client.GetAsync($"/children/selected", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Equal(childName2, content.Name);
                    Assert.Equal(child2Guid, content.ChildId.ToString());
                    Assert.True(content.Selected);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.DeleteAsync($"/children/{child2Guid}/remove", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName1 && c.Selected == true);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(async () => await Client.GetAsync($"/children/all", username),
                HttpStatusCode.OK,
                (content) => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName1 && c.ChildId.ToString() == child1Guid && c.Selected == true);
                });
        }

        // TODO - Add more classroom code edit validation when classrooms are added

    }
}