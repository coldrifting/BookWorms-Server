using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;

using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ChildBookshelfTests(BaseStartup<Program> factory) : BaseTest(factory)
{
    [Fact]
    public async Task Test_GetAllBookshelves_AsLoggedOutShouldError()
    {
        await CheckForError(() => Client.GetAsync($"/bookshelves/all"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("teacher1")]
    public async Task Test_GetAllBookshelves_AsTeacherShouldError(string username)
    {
        await CheckForError(() => Client.GetAsyncAsUser($"/bookshelves/all", username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0")]
    public async Task Test_GetAllBookshelves_NoChildrenShouldError(string username)
    {
        await CheckForError(() => Client.GetAsyncAsUser($"/bookshelves/all", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("bookshelf")]
    public async Task Test_GetBookshelf_AsLoggedOutShouldError(string bookshelfName)
    {
        await CheckForError(() => Client.GetAsync($"/bookshelves/{bookshelfName}/details"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("teacher1", "bookshelf")]
    public async Task Test_GetBookshelf_AsTeacherShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.GetAsyncAsUser($"/bookshelves/{bookshelfName}/details", username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0", "bookshelfName")]
    public async Task Test_GetBookshelf_NoChildrenShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.GetAsyncAsUser($"/bookshelves/{bookshelfName}/details", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("parent1", "bookshelfName")]
    public async Task Test_GetBookshelf_BookshelfNotExist_ShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.GetAsyncAsUser($"/bookshelves/{bookshelfName}/details", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfNotFound);
    }
    
    [Theory]
    [InlineData("bookshelf")]
    public async Task Test_AddBookshelf_AsLoggedOutShouldError(string bookshelfName)
    {
        await CheckForError(() => Client.PostAsync($"/bookshelves/{bookshelfName}/add"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("teacher1", "bookshelf")]
    public async Task Test_AddBookshelf_AsTeacherShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.PostAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/add",  new{}, username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0", "bookshelfName")]
    public async Task Test_AddBookshelf_NoChildrenShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.PostAsyncAsUser($"/bookshelves/{bookshelfName}/add", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)")]
    public async Task Test_AddBookshelf_BookshelfNameAlreadyExists(string username, string bookshelfName)
    {
        await CheckForError(() => Client.PostAsyncAsUser($"/bookshelves/{bookshelfName}/add", username), 
            HttpStatusCode.UnprocessableEntity, 
            ErrorDTO.BookshelfAlreadyExists);
    }
    
    [Theory]
    [InlineData("bookshelf")]
    public async Task Test_RemoveBookshelf_AsLoggedOutShouldError(string bookshelfName)
    {
        await CheckForError(() => Client.DeleteAsync($"/bookshelves/{bookshelfName}/delete"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("teacher1", "bookshelf")]
    public async Task Test_RemoveBookshelf_AsTeacherShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/delete",  username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0", "bookshelfName")]
    public async Task Test_RemoveBookshelf_NoChildrenShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/delete", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("bookshelf", "newBookshelf")]
    public async Task Test_RenameBookshelf_AsLoggedOutShouldError(string bookshelfName, string newBookshelfName)
    {
        await CheckForError(() => Client.PostAsync($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}"), 
            HttpStatusCode.Unauthorized, 
            ErrorDTO.Unauthorized);
    }
    
    [Theory]
    [InlineData("teacher1", "bookshelf", "newBookshelf")]
    public async Task Test_RenameBookshelf_AsTeacherShouldError(string username, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(() => Client.PostAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}", new {}, username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0", "bookshelfName", "newBookshelf")]
    public async Task Test_RenameBookshelf_NoChildrenShouldError(string username, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(() => Client.PostAsyncAsUser($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("parent1", "invalidBookshelf", "newBookshelfName")]
    public async Task Test_RenameBookshelf_BookshelfNotExist(string username, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(() => Client.PostAsyncAsUser($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfNotFound);
    }
    
    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "Evelyn_books")]
    public async Task Test_RenameBookshelf_BookshelfNameAlreadyExists(string username, string bookshelfName1, string bookshelfName2)
    {
        var msg = await Client.PostAsyncAsUser($"/bookshelves/{bookshelfName2}/add", username);
        var con = await msg.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        
        await CheckForError(() => Client.PostAsyncAsUser($"/bookshelves/{bookshelfName1}/rename?newName={bookshelfName2}", username), 
            HttpStatusCode.UnprocessableEntity, 
            ErrorDTO.BookshelfAlreadyExists);
    }
    
    [Theory]
    [InlineData("parent3", "1560bea2-7dcd-4b87-a9d3-e89012262270", "Books")]
    public async Task Test_AddBookshelf_Basic_ChildWithNoBookshelves(string username, Guid childId, string bookshelfName)
    {
        await Client.PostAsyncAsUser($"/children/{childId}/select", username);
        
        HttpResponseMessage response1 = await Client.PostAsyncAsUser($"/bookshelves/{bookshelfName}/add", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var content1 = await response1.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/bookshelves/all", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var content2 = await response2.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.Equal(content1, content2);
        Assert.NotNull(content2);
        Assert.Single(content2);
        Assert.NotNull(content2[0]);
        Assert.Equal(content2[0].Name, bookshelfName);
        Assert.Empty(content2[0].Books);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/bookshelves/{bookshelfName}/details", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);
        
        var content4 = await response4.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content4);
        Assert.Equal(content4.Name, bookshelfName);
        Assert.Empty(content4.Books);
    }
    
    
    [Theory]
    [InlineData("parent0", "parent2", "alice", "alice", "book", "shelf", "OL3368288W")]
    [InlineData("parent0", "parent2", "alice", "bob", "books", "books", "OL3368288W")]
    [InlineData("parent0", "parent2", "alice", "alice", "books", "books", "OL3368288W")]
    public async Task Test_AddBookshelf_SameChildOrBookshelfNameDifParent(
        string username1, string username2, 
        string childName1, string childName2, 
        string bookshelfName1, string bookshelfName2,
        string bookId)
    {
        HttpResponseMessage responseA = await Client.PostAsyncAsUser($"/children/add?childName={childName1}", username1);
        string? child1Guid = responseA.Headers.Location?.ToString().Replace("/children/", "");
        await Client.PutAsyncAsUser($"/children/{child1Guid}/select", username1);
        
        HttpResponseMessage responseB = await Client.PostAsyncAsUser($"/children/add?childName={childName2}", username2);
        string? child2Guid = responseB.Headers.Location?.ToString().Replace("/children/", "");
        await Client.PutAsyncAsUser($"/children/{child2Guid}/select", username2);
        
        HttpResponseMessage response1 = await Client.PostAsyncAsUser($"/bookshelves/{bookshelfName1}/add", username1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        await Client.PutAsyncAsUser($"/bookshelves/{bookshelfName1}/insert/{bookId}", username1);
        
        HttpResponseMessage response2 = await Client.PostAsyncAsUser($"/bookshelves/{bookshelfName2}/add", username2);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/bookshelves/{bookshelfName1}/details", username1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        BookshelfPreviewResponseDTO? content3 = await response3.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content3);
        Assert.Equal(content3.Name, bookshelfName1);
        Assert.Single(content3.Books);
        Assert.Contains(content3.Books, c => c.BookId == bookId);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/bookshelves/all", username1);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var content4 = await response4.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content4);
        Assert.Single(content4);
        Assert.NotNull(content4[0]);
        Assert.Equal(content4[0].Name, bookshelfName1);
        Assert.Contains(content4[0].Books, c => c.BookId == bookId);
        
        HttpResponseMessage response5 = await Client.GetAsyncAsUser($"/bookshelves/{bookshelfName2}/details", username2);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

        BookshelfPreviewResponseDTO? content5 = await response5.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content5);
        Assert.Equal(content5.Name, bookshelfName2);
        Assert.Empty(content5.Books);
        
        HttpResponseMessage response6 = await Client.GetAsyncAsUser($"/bookshelves/all", username2);
        Assert.Equal(HttpStatusCode.OK, response6.StatusCode);

        var content6 = await response6.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content6);
        Assert.Single(content6);
        Assert.NotNull(content6[0]);
        Assert.Equal(content6[0].Name, bookshelfName2);
        Assert.Empty(content6[0].Books);
    }

    [Theory]
    [InlineData("parent3", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f", "Costanza's curation")]
    public async Task Test_DeleteBookshelf_BasicWithoutBooks(string username, Guid childid, string bookshelfName)
    {
        await Client.PutAsyncAsUser($"/children/{childid}/select", username);
        
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/delete", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var content1 = await response1.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content1);
        Assert.Empty(content1);
    }

    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)")]
    public async Task Test_DeleteBookshelf_BasicWithBooks(string username, string bookshelfName)
    {
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/delete", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Empty(content3);
    }

    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "bookshelf2")]
    public async Task Test_DeleteBookshelf_MultipleSameChild(string username, string existingBookshelf, string newBookshelf)
    {
        await Client.PostAsyncAsUser($"/bookshelves/{newBookshelf}/add", username);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/bookshelves/{existingBookshelf}/delete", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Single(content3);
        Assert.NotNull(content3[0]);
        Assert.Equal(content3[0].Name, newBookshelf);
        Assert.Empty(content3[0].Books);
    }
    
    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "NEW books")]
    public async Task Test_RenameBookshelf_Basic(string username, string oldBookshelfName, string newBookshelfName)
    {
        HttpResponseMessage response1 = await Client.PostAsyncAsUser($"/bookshelves/{oldBookshelfName}/rename?newName={newBookshelfName}", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var content1 = await response1.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        
        HttpResponseMessage response2 = await Client.GetAsyncAsUser($"/bookshelves/all", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var content2 = await response2.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        
        Assert.Equal(content1, content2);
        Assert.NotNull(content2);
        Assert.Single(content2);
        Assert.NotNull(content2[0]);
        Assert.Equal(content2[0].Name, newBookshelfName);
        Assert.Equal(3, content2[0].Books.Length);
    }
    
    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "NEW books")]
    public async Task Test_RenameBookshelf_BookshelfAlreadyExists(string username, string oldBookshelfName, string newBookshelfName)
    {
        await Client.PostAsJsonAsyncAsUser($"/bookshelves/{newBookshelfName}/add", new{}, username);
        
        await CheckForError(() => Client.PostAsyncAsUser($"/bookshelves/{newBookshelfName}/rename?newName={oldBookshelfName}", username), 
            HttpStatusCode.UnprocessableEntity, 
            ErrorDTO.BookshelfAlreadyExists);
        
        HttpResponseMessage response3 = await Client.GetAsyncAsUser($"/bookshelves/all", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Equal(2, content3.Count);
        Assert.Contains(content3, b => b.Name == oldBookshelfName);
        Assert.Contains(content3, b => b.Name == newBookshelfName);
    }
    
    [Theory]
    [InlineData("parent1", "parent2", "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
    public async Task Test_RenameBookshelf_BookshelfSameNameSameChildNameDiffParents(string username1, string username2, string bookshelfName1, string bookshelfName2)
    {
        HttpResponseMessage response3 =
            await Client.PostAsyncAsUser($"/bookshelves/{bookshelfName1}/rename?newName={bookshelfName2}", username1);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        
        HttpResponseMessage response4 =
            await Client.GetAsyncAsUser($"/bookshelves/all", username1);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var content4 = await response4.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.Equal(content3, content4);
        Assert.NotNull(content4);
        Assert.Single(content4);
        Assert.Contains(content4, b => b.Name == bookshelfName2);

        HttpResponseMessage response5 =
            await Client.GetAsyncAsUser($"/bookshelves/all", username2);
        Assert.Equal(HttpStatusCode.OK, response5.StatusCode);

        var content5 = await response5.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content5);
        Assert.Single(content5);
        Assert.Contains(content5, b => b.Name == bookshelfName2);
    }
    
    [Theory]
    [InlineData("parent1", "parent2", "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
    public async Task Test_DeleteBookshelf_BookshelfSameNameSameChildNameDiffParents(string username1, string username2, string bookshelfName1, string bookshelfName2)
    {
        HttpResponseMessage response1 =
            await Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName1}/delete", username1);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var content1 = await response1.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        
        HttpResponseMessage response2 =
            await Client.GetAsyncAsUser($"/bookshelves/all", username1);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);

        var content2 = await response2.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.Equal(content1, content2);
        Assert.NotNull(content2);
        Assert.Empty(content2);

        HttpResponseMessage response3 =
            await Client.GetAsyncAsUser($"/bookshelves/all", username2);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Single(content3);
        Assert.Contains(content3, b => b.Name == bookshelfName2);
    }
    
    [Theory]
    [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "1560bea2-7dcd-4b87-a9d3-e89012262270", "Eric's books")]
    public async Task Test_RemoveBookshelf_SameParentDiffChildSameBookshelfName(string username, Guid childId1, Guid childId2, string bookshelfName)
    {
        await Client.PutAsyncAsUser($"/children/{childId2}/select", username);
        await Client.PostAsyncAsUser($"/bookshelves/{bookshelfName}/add", username);
        
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/delete", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content3);
        Assert.Empty(content3);
        
        await Client.PutAsyncAsUser($"/children/{childId1}/select", username);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/bookshelves/all", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var content4 = await response4.Content.ReadJsonAsync<List<BookshelfPreviewResponseDTO>>();
        Assert.NotNull(content4);
        Assert.Single(content4);
        Assert.Contains(content4, b => b.Name == bookshelfName);
    }
    
    [Theory]
    [InlineData("teacher1", "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_NotParent_ShouldError(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.PutAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId}", username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0", "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_NoChildren_ShouldError(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.PutAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId}", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("parent1", "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_BookshelfNameNotExist_ShouldError(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.PutAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId}", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfNotFound);
    }
    
    [Theory]
    [InlineData("parent5", "Ryn's Empty Bookshelf", "InvalidBookId")]
    public async Task Test_BookshelfInsert_BookIdNotValid_ShouldError(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.PutAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId}", username), 
            HttpStatusCode.UnprocessableEntity, 
            ErrorDTO.BookNotFound);
    }
    
    [Theory]
    [InlineData("parent5", "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_EmptyBookshelf(string username,string bookshelfName, string bookId)
    {
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId}", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var content1 = await response1.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content1);
        Assert.Equal(bookshelfName, content1.Name);
        Assert.Single(content1.Books);
        Assert.Contains(content1.Books, b => b.BookId == bookId);
    }
    
    [Theory]
    [InlineData("parent4", "Madison's collection", "OL3368273W", "OL48763W")]
    public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookNotAlreadyInBookshelf(string username,string bookshelfName, string bookId, string existingBookId)
    {
        HttpResponseMessage response3 = await Client.PutAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId}", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content3);
        Assert.Equal(bookshelfName, content3.Name);
        Assert.Contains(content3.Books, b => b.BookId == bookId);
        Assert.Contains(content3.Books, b => b.BookId == existingBookId);
    }
    
    [Theory]
    [InlineData("parent4", "Madison's collection", "OL48763W")]
    public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookAlreadyInBookshelf(string username,string bookshelfName, string bookId)
    {
        HttpResponseMessage response3 = await Client.PutAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId}", username);
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);

        var content3 = await response3.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content3);
        Assert.Equal(bookshelfName, content3.Name);
        Assert.Contains(content3.Books, b => b.BookId == bookId);
    }
    
    [Theory]
    [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL3368286W", "OL48763W")]
    public async Task Test_BookshelfInsert_Multiple_NeitherAlreadyExists(string username,  Guid childId, string bookshelfName, string bookId1, string bookId2)
    {
        await Client.PutAsyncAsUser($"/children/{childId}/select", username);
        
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId1}", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var content1 = await response1.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content1);
        Assert.Equal(bookshelfName, content1.Name);
        Assert.Equal(3, content1.Books.Length);
        Assert.Contains(content1.Books, b => b.BookId == bookId1);
        
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId2}", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var content2 = await response2.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content2);
        Assert.Equal(bookshelfName, content2.Name);
        Assert.Equal(4, content2.Books.Length);
        Assert.Contains(content2.Books, b => b.BookId == bookId1);
        Assert.Contains(content2.Books, b => b.BookId == bookId2);
    }
    
    [Theory]
    [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL28633459W", "OL48763W")]
    public async Task Test_BookshelfInsert_Multiple_OneAlreadyExists(string username, Guid childId, string bookshelfName, string bookId1, string bookId2)
    {
        await Client.PutAsyncAsUser($"/children/{childId}/select", username);
        
        HttpResponseMessage response1 = await Client.PutAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId1}", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var content1 = await response1.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content1);
        Assert.Equal(bookshelfName, content1.Name);
        Assert.Equal(2, content1.Books.Length);
        Assert.Contains(content1.Books, b => b.BookId == bookId1);
        
        HttpResponseMessage response2 = await Client.PutAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId2}", username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var content2 = await response2.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content2);
        Assert.Equal(bookshelfName, content2.Name);
        Assert.Equal(3, content2.Books.Length);
        Assert.Contains(content2.Books, b => b.BookId == bookId1);
        Assert.Contains(content2.Books, b => b.BookId == bookId2);
    }
    
    [Theory]
    [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL28633459W", "OL3368273W")]
    public async Task Test_BookshelfInsert_Multiple_BothAlreadyExists(string username, Guid childId, string bookshelfName, string bookId1, string bookId2)
    {
        await Client.PutAsyncAsUser($"/children/{childId}/select", username);
        
        HttpResponseMessage response1 = await Client.PutAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId1}", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        
        var content1 = await response1.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content1);
        Assert.Equal(bookshelfName, content1.Name);
        Assert.Equal(2, content1.Books.Length);
        Assert.Contains(content1.Books, b => b.BookId == bookId1);
        Assert.Contains(content1.Books, b => b.BookId == bookId2);
        
        HttpResponseMessage response2 = await Client.PutAsJsonAsyncAsUser($"/bookshelves/{bookshelfName}/insert/{bookId2}", new {}, username);
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        
        var content2 = await response2.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content2);
        Assert.Equal(bookshelfName, content2.Name);
        Assert.Equal(2, content2.Books.Length);
        Assert.Contains(content2.Books, b => b.BookId == bookId1);
        Assert.Contains(content2.Books, b => b.BookId == bookId2);
    }
    
    [Theory]
    [InlineData("teacher1", "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfRemove_NotParent_ShouldError(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/remove/{bookId}", username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0", "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfRemove_NoChildren_ShouldError(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/remove/{bookId}", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("parent1", "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfDelete_BookshelfNotFound_ShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/delete", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfNotFound);
    }
    
    [Theory]
    [InlineData("parent2", "Not a valid bookshelf", "OL3368273W")]
    public async Task Test_RemoveBookshelfItem_InvalidBookshelf(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/remove/{bookId}", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfNotFound);
    }
    
    [Theory]
    [InlineData("parent5", "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_RemoveBookshelfItem_EmptyBookshelf(string username, string bookshelfName, string bookId)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/remove/{bookId}", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfBookNotFound);
    }
    
    [Theory]
    [InlineData("parent4", "Madison's collection", "invalidBookId")]
    public async Task Test_RemoveBookshelfItem_InvalidBookId(string username,string bookshelfName, string invalidBookId)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/remove/{invalidBookId}", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfBookNotFound);
    }
    
    [Theory]
    [InlineData("parent4", "Madison's collection", "OL48763W")]
    public async Task Test_RemoveBookshelfItem_SingleItemInBookshelf(string username,string bookshelfName, string bookId)
    {
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/remove/{bookId}", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var content1 = await response1.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content1);
        Assert.Equal(bookshelfName, content1.Name);
        Assert.Empty(content1.Books);
    }
    
    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL3368288W")]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL48763W")]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL28633459W")]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL3368286W")]
    public async Task Test_RemoveBookshelfItem_MultipleItemsInBookshelf(string username,string bookshelfName, string bookId)
    {
        HttpResponseMessage response1 = await Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/remove/{bookId}", username);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var content1 = await response1.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content1);
        Assert.Equal(3, content1.Books.Length);
        Assert.DoesNotContain(content1.Books, c => c.BookId == bookId);
    }
    
    [Theory]
    [InlineData("teacher1", "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfClear_NotParent_ShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/clear", username), 
            HttpStatusCode.Forbidden, 
            ErrorDTO.UserNotParent);
    }
    
    [Theory]
    [InlineData("parent0", "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfClear_NoChildren_ShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/clear", username), 
            HttpStatusCode.BadRequest, 
            ErrorDTO.NoChildSelected);
    }
    
    [Theory]
    [InlineData("parent1", "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfClear_BookshelfNotFound_ShouldError(string username, string bookshelfName)
    {
        await CheckForError(() => Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/clear", username), 
            HttpStatusCode.NotFound, 
            ErrorDTO.BookshelfNotFound);
    }
    
    [Theory]
    [InlineData("parent1", "Evelyn's bookshelf (Parent1)")]
    public async Task Test_ClearBookshelf_Basic(string username, string bookshelfName)
    {
        HttpResponseMessage response3 = await Client.DeleteAsyncAsUser($"/bookshelves/{bookshelfName}/clear", username);
        Assert.Equal(HttpStatusCode.NoContent, response3.StatusCode);
        
        HttpResponseMessage response4 = await Client.GetAsyncAsUser($"/bookshelves/{bookshelfName}/details", username);
        Assert.Equal(HttpStatusCode.OK, response4.StatusCode);

        var content4 = await response4.Content.ReadJsonAsync<BookshelfPreviewResponseDTO>();
        Assert.NotNull(content4);
        Assert.Equal(bookshelfName, content4.Name);
        Assert.Empty(content4.Books);
    }
    
}