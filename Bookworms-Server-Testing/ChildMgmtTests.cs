using System.Net;
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
            await CheckForError(
                () => Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0")]
        public async Task Test_GetAllChildren_NoChildren(string username)
        {
            HttpResponseMessage response = await Client.GetAsync(Routes.Children.All, username);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var children = await response.Content.ReadJsonAsync<List<ChildResponseDTO>>();
            Assert.NotNull(children);
            Assert.Empty(children);
        }

        [Theory]
        [InlineData("joey")]
        public async Task Test_AddChild_NotLoggedIn(string childName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Children.Add(childName)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "joey")]
        [InlineData("admin", "joey")]
        public async Task Test_AddChild_NotAParent(string username, string childName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Children.Add(childName), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent1", "389b78f0-13f1-4003-8be4-9a72cb145d9e", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
        public async Task Test_RemoveChild_ChildNotExist(string username, Guid invalidChildId, Guid existingChildId)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Children.Remove(invalidChildId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == existingChildId);
                });
        }

        [Theory]
        [InlineData("teacher1", "389b78f0-13f1-4003-8be4-9a72cb145d9e")]
        public async Task Test_RemoveChild_NotParent(string username, Guid childId)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Children.Remove(childId), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("teacher1", "e1740f4a-9855-472d-bf47-fb57dce6c1b2")]
        public async Task Test_EditChild_NotParent(string username, Guid childId)
        {
            await CheckForError(
                () => Client.PutPayloadAsync(Routes.Children.Edit(childId), new ChildEditDTO(), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }
        
        [Theory]
        [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "BadVal")]
        public async Task Test_EditChild_InvalidClassroomCode(string username, Guid childId, string classroomCode)
        {
            await CheckForError(
                () => Client.PutPayloadAsync(Routes.Children.Edit(childId),
                    new ChildEditDTO(ClassroomCode: classroomCode, ReadingLevel: "A5"), username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.ClassroomNotFound);
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId == childId && 
                                                                       c.ClassroomCode is null && 
                                                                       c.ReadingLevel == null);
                });
        }

        [Theory]
        [InlineData("parent2", "c5dca20d-1234-418f-b65b-cb79aa723c20", "newName")]
        public async Task Test_EditChild_InvalidGuid(string username, Guid childId, string newName)
        {
            await CheckForError(
                () => Client.PutPayloadAsync(Routes.Children.Edit(childId),
                    new ChildEditDTO(NewName: newName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }
    }
    
    [Collection("Integration Tests")]
    public class ChildMgmtWriteTests(AppFactory<Program> factory) : BaseTestWriteFixture(factory)
    {
        [Theory]
        [InlineData("parent0", "Jason")]
        public async Task Test_AddChild_First(string username, string childName)
        {
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.PostAsync(Routes.Children.Add(childName), username),
                HttpStatusCode.Created,
                (content, headers) => {
                    Guid? childGuid = headers.GetChildLocation();
                    Assert.NotNull(childGuid);
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childGuid);
                });
        }
        
        [Theory]
        [InlineData("parent1", "Jason")]
        public async Task Test_AddChild_Second(string username, string childName)
        {
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.PostAsync(Routes.Children.Add(childName), username),
                HttpStatusCode.Created,
                (content, headers) => {
                    Guid? childGuid = headers.GetChildLocation();
                    Assert.NotNull(childGuid);
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId == childGuid);
                });
        }

        [Theory]
        [InlineData("parent0", "joey", "ash")]
        [InlineData("parent0", "joey", "joey")]
        public async Task Test_AddChild_MultipleSameParent(string username, string childName1, string childName2)
        {
            Guid childGuid1 = await CheckResponse<List<ChildResponseDTO>, Guid>(
                async () => await Client.PostAsync(Routes.Children.Add(childName1), username),
                HttpStatusCode.Created,
                (content, headers) => {
                    Guid? childGuid = headers.GetChildLocation();
                    Assert.NotNull(childGuid);
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childGuid && c.Name == childName1);
                    return childGuid.Value;
                });
            
            Guid childGuid2 = await CheckResponse<List<ChildResponseDTO>, Guid>(
                async () => await Client.PostAsync(Routes.Children.Add(childName2), username),
                HttpStatusCode.Created,
                (content, headers) => {
                    Guid? childGuid = headers.GetChildLocation();
                    Assert.NotNull(childGuid);
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId == childGuid && c.Name == childName2);
                    Assert.Contains(content, c => c.ChildId == childGuid1 && c.Name == childName1);
                    return childGuid.Value;
                });
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.ChildId == childGuid1 && c.Name == childName1);
                    Assert.Contains(content, c => c.ChildId == childGuid2 && c.Name == childName2);
                });
        }

        [Theory]
        [InlineData("parent0", "parent5", "joey")]
        public async Task Test_AddChild_MultipleSameName_DifferentParents(string username1, string username2,
            string childName)
        {
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.PostAsync(Routes.Children.Add(childName), username1),
                HttpStatusCode.Created,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.PostAsync(Routes.Children.Add(childName), username2),
                HttpStatusCode.Created,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name != childName);
                    Assert.Contains(content, c => c.Name == childName);
                });

            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.Name == childName);
                });

            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name != childName);
                    Assert.Contains(content, c => c.Name == childName);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229")]
        public async Task Test_RemoveChild_Basic(string username, Guid childId)
        {
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.DeleteAsync(Routes.Children.Remove(childId), username),
                HttpStatusCode.OK,
                Assert.Empty);
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                Assert.Empty);
        }

        [Theory]
        [InlineData("parent2", "c5dca20d-849d-418f-b65b-cb79aa723c20", "08dd3c4b-f197-4657-8556-58c76701802b")]
        public async Task Test_RemoveChild_DoesNotDeleteOtherChildren(string username, Guid childToRemoveId, Guid childLeftId)
        {
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.DeleteAsync(Routes.Children.Remove(childToRemoveId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childLeftId);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childLeftId);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Rachel")]
        public async Task Test_EditChild_ChangeName_Basic(string username, Guid childId, string newName)
        {
            await CheckResponse<ChildResponseDTO>(
                async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId), 
                    new ChildEditDTO(newName), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(newName, content.Name);
                });

            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childId && c.Name == newName);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", 2)]
        public async Task Test_EditChild_ChangeIcon_Basic(string username, Guid childId, int newIcon)
        {
            await CheckResponse<ChildResponseDTO>(
                async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId),
                new ChildEditDTO(ChildIcon: newIcon), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(newIcon, content.ChildIcon);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(3, content.Count);
                    Assert.Contains(content, c => c.ChildId == childId && c.ChildIcon == newIcon);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f",
            "Costanza")]
        public async Task Test_EditChild_ChangeName_NameAlreadyExistsUnderParent(string username, Guid childId, Guid childId2, string newName)
        {
            await CheckResponse<ChildResponseDTO>(
                async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId),
                new ChildEditDTO(NewName: newName), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(newName, content.Name);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(3, content.Count);
                    Assert.Contains(content, c => c.Name == newName && c.ChildId == childId);
                    Assert.Contains(content, c => c.Name == newName && c.ChildId == childId2);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "A4")]
        public async Task Test_EditChild_ChangeReadingLevel_Basic(string username, Guid childId, string readingLevel)
        {
            await CheckResponse<ChildResponseDTO>(
                async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId),
                new ChildEditDTO(ReadingLevel: readingLevel), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(readingLevel, content.ReadingLevel);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, c => c.ChildId == childId && c.ReadingLevel == readingLevel);
                });
        }

        [Theory]
        [InlineData("parent3", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f", "1960-08-31")]
        public async Task Test_EditChild_ChangeDOB_Basic(string username, Guid childId, string dob)
        {
            await CheckResponse<ChildResponseDTO>(
                async () => await Client.PutPayloadAsync(Routes.Children.Edit(childId),
                new ChildEditDTO(DateOfBirth: DateOnly.Parse(dob)), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(childId, content.ChildId);
                    Assert.Equal(DateOnly.Parse(dob), content.DateOfBirth);
                });
            
            await CheckResponse<List<ChildResponseDTO>>(
                async () => await Client.GetAsync(Routes.Children.All, username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(3, content.Count);
                    Assert.Contains(content, c => c.ChildId == childId && c.DateOfBirth == DateOnly.Parse(dob));
                });
        }

        // TODO - Add more classroom code edit validation when classrooms are added

    }
}