using System.Net;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Templates;
using static BookwormsServerTesting.Templates.Common;

namespace BookwormsServerTesting;

public abstract class ChildBookshelfTests
{
    [Collection("Integration Tests")]
    public class ChildBookshelfReadOnlyTests(AppFactory<Program> factory) : BaseTestReadOnlyFixture(factory)
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
            await CheckForError(() => Client.GetAsync($"/bookshelves/all", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0")]
        public async Task Test_GetAllBookshelves_NoChildrenShouldError(string username)
        {
            await CheckForError(() => Client.GetAsync($"/bookshelves/all", username),
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
            await CheckForError(() => Client.GetAsync($"/bookshelves/{bookshelfName}/details", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "bookshelfName")]
        public async Task Test_GetBookshelf_NoChildrenShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.GetAsync($"/bookshelves/{bookshelfName}/details", username),
                HttpStatusCode.BadRequest,
                ErrorDTO.NoChildSelected);
        }

        [Theory]
        [InlineData("parent1", "bookshelfName")]
        public async Task Test_GetBookshelf_BookshelfNotExist_ShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.GetAsync($"/bookshelves/{bookshelfName}/details", username),
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
            await CheckForError(
                () => Client.PostAsync($"/bookshelves/{bookshelfName}/add", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "bookshelfName")]
        public async Task Test_AddBookshelf_NoChildrenShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.PostAsync($"/bookshelves/{bookshelfName}/add", username),
                HttpStatusCode.BadRequest,
                ErrorDTO.NoChildSelected);
        }

        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)")]
        public async Task Test_AddBookshelf_BookshelfNameAlreadyExists(string username, string bookshelfName)
        {
            await CheckForError(() => Client.PostAsync($"/bookshelves/{bookshelfName}/add", username),
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
            await CheckForError(() => Client.DeleteAsync($"/bookshelves/{bookshelfName}/delete", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "bookshelfName")]
        public async Task Test_RemoveBookshelf_NoChildrenShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.DeleteAsync($"/bookshelves/{bookshelfName}/delete", username),
                HttpStatusCode.BadRequest,
                ErrorDTO.NoChildSelected);
        }

        [Theory]
        [InlineData("bookshelf", "newBookshelf")]
        public async Task Test_RenameBookshelf_AsLoggedOutShouldError(string bookshelfName, string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}"),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "bookshelf", "newBookshelf")]
        public async Task Test_RenameBookshelf_AsTeacherShouldError(string username, string bookshelfName,
            string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "bookshelfName", "newBookshelf")]
        public async Task Test_RenameBookshelf_NoChildrenShouldError(string username, string bookshelfName,
            string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}",
                    username),
                HttpStatusCode.BadRequest,
                ErrorDTO.NoChildSelected);
        }

        [Theory]
        [InlineData("parent1", "invalidBookshelf", "newBookshelfName")]
        public async Task Test_RenameBookshelf_BookshelfNotExist(string username, string bookshelfName,
            string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync($"/bookshelves/{bookshelfName}/rename?newName={newBookshelfName}",
                    username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("teacher1", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_NotParent_ShouldError(string username, string bookshelfName,
            string bookId)
        {
            await CheckForError(() => Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId}", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_NoChildren_ShouldError(string username, string bookshelfName,
            string bookId)
        {
            await CheckForError(() => Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId}", username),
                HttpStatusCode.BadRequest,
                ErrorDTO.NoChildSelected);
        }

        [Theory]
        [InlineData("parent1", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_BookshelfNameNotExist_ShouldError(string username, string bookshelfName,
            string bookId)
        {
            await CheckForError(() => Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId}", username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("parent5", "Ryn's Empty Bookshelf", "InvalidBookId")]
        public async Task Test_BookshelfInsert_BookIdNotValid_ShouldError(string username, string bookshelfName,
            string bookId)
        {
            await CheckForError(() => Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId}", username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.BookNotFound);
        }

        [Theory]
        [InlineData("teacher1", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfRemove_NotParent_ShouldError(string username, string bookshelfName,
            string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync($"/bookshelves/{bookshelfName}/remove/{bookId}", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfRemove_NoChildren_ShouldError(string username, string bookshelfName,
            string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync($"/bookshelves/{bookshelfName}/remove/{bookId}", username),
                HttpStatusCode.BadRequest,
                ErrorDTO.NoChildSelected);
        }

        [Theory]
        [InlineData("parent1", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfDelete_BookshelfNotFound_ShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.DeleteAsync($"/bookshelves/{bookshelfName}/delete", username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("parent2", "Not a valid bookshelf", "OL3368273W")]
        public async Task Test_RemoveBookshelfItem_InvalidBookshelf(string username, string bookshelfName,
            string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync($"/bookshelves/{bookshelfName}/remove/{bookId}", username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("parent5", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_RemoveBookshelfItem_EmptyBookshelf(string username, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync($"/bookshelves/{bookshelfName}/remove/{bookId}", username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfBookNotFound);
        }

        [Theory]
        [InlineData("parent4", "Madison's collection", "invalidBookId")]
        public async Task Test_RemoveBookshelfItem_InvalidBookId(string username, string bookshelfName,
            string invalidBookId)
        {
            await CheckForError(
                () => Client.DeleteAsync($"/bookshelves/{bookshelfName}/remove/{invalidBookId}", username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfBookNotFound);
        }

        [Theory]
        [InlineData("teacher1", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfClear_NotParent_ShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.DeleteAsync($"/bookshelves/{bookshelfName}/clear", username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfClear_NoChildren_ShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.DeleteAsync($"/bookshelves/{bookshelfName}/clear", username),
                HttpStatusCode.BadRequest,
                ErrorDTO.NoChildSelected);
        }

        [Theory]
        [InlineData("parent1", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfClear_BookshelfNotFound_ShouldError(string username, string bookshelfName)
        {
            await CheckForError(() => Client.DeleteAsync($"/bookshelves/{bookshelfName}/clear", username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }
    }

    [Collection("Integration Tests")]
    public class ChildBookshelfWriteTests(AppFactory<Program> factory) : BaseTestWriteFixture(factory)
    {
        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "Evelyn_books")]
        public async Task Test_RenameBookshelf_BookshelfNameAlreadyExists(string username, string bookshelfName1,
            string bookshelfName2)
        {
            await Client.PostAsync($"/bookshelves/{bookshelfName2}/add", username);

            await CheckForError(
                () => Client.PostAsync($"/bookshelves/{bookshelfName1}/rename?newName={bookshelfName2}",
                    username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.BookshelfAlreadyExists);
        }

        [Theory]
        [InlineData("parent3", "1560bea2-7dcd-4b87-a9d3-e89012262270", "Books")]
        public async Task Test_AddBookshelf_Basic_ChildWithNoBookshelves(string username, Guid childId,
            string bookshelfName)
        {
            await Client.PostAsync($"/children/{childId}/select", username);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.PostAsync($"/bookshelves/{bookshelfName}/add", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName);
                    Assert.Empty(content[0].Books);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName);
                    Assert.Empty(content[0].Books);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.GetAsync($"/bookshelves/{bookshelfName}/details", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(content.Name, bookshelfName);
                    Assert.Empty(content.Books);
                });
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
            string child1Guid = await CheckResponse<List<ChildResponseDTO>, string>(async () => await Client.PostAsync($"/children/add?childName={childName1}", username1),
                HttpStatusCode.Created,
                (_, headers) => {
                    string? childGuid = headers.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(childGuid);
                    Assert.NotEmpty(childGuid);
                    return childGuid;
                });
            await Client.PutAsync($"/children/{child1Guid}/select", username1);
            
            string child2Guid = await CheckResponse<List<ChildResponseDTO>, string>(async () => await Client.PostAsync($"/children/add?childName={childName2}", username2),
                HttpStatusCode.Created,
                (_, headers) => {
                    string? childGuid = headers.Location?.ToString().Replace("/children/", "");
                    Assert.NotNull(childGuid);
                    Assert.NotEmpty(childGuid);
                    return childGuid;
                });
            await Client.PutAsync($"/children/{child2Guid}/select", username2);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.PostAsync($"/bookshelves/{bookshelfName1}/add", username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName1);
                    Assert.Empty(content[0].Books);
                });

            // Insert a book to distinguish the two bookshelves
            await Client.PutAsync($"/bookshelves/{bookshelfName1}/insert/{bookId}", username1);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.PostAsync($"/bookshelves/{bookshelfName2}/add", username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName2);
                    Assert.Empty(content[0].Books);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.GetAsync($"/bookshelves/{bookshelfName1}/details", username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(content.Name, bookshelfName1);
                    Assert.Single(content.Books);
                    Assert.Contains(content.Books, c => c.BookId == bookId);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName1);
                    Assert.Contains(content[0].Books, c => c.BookId == bookId);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.GetAsync($"/bookshelves/{bookshelfName2}/details", username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(content.Name, bookshelfName2);
                    Assert.Empty(content.Books);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName2);
                    Assert.Empty(content[0].Books);
                });
        }

        [Theory]
        [InlineData("parent3", "3eda09c6-53ee-44a4-b784-fbd90d5b7b1f", "Costanza's curation")]
        public async Task Test_DeleteBookshelf_BasicWithoutBooks(string username, Guid childid, string bookshelfName)
        {
            await Client.PutAsync($"/children/{childid}/select", username);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.DeleteAsync($"/bookshelves/{bookshelfName}/delete", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Empty(content);
                });
        }

        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)")]
        public async Task Test_DeleteBookshelf_BasicWithBooks(string username, string bookshelfName)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.DeleteAsync($"/bookshelves/{bookshelfName}/delete", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Empty(content);
                });
        }

        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "bookshelf2")]
        public async Task Test_DeleteBookshelf_DeleteSingle_MultipleBookshelvesExistSameChild(string username, string existingBookshelf, string newBookshelf)
        {
            await Client.PostAsync($"/bookshelves/{newBookshelf}/add", username);

            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.DeleteAsync($"/bookshelves/{existingBookshelf}/delete", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, newBookshelf);
                    Assert.Empty(content[0].Books);
                });
        }

        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "NEW books")]
        public async Task Test_RenameBookshelf_Basic(string username, string oldBookshelfName, string newBookshelfName)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.PostAsync($"/bookshelves/{oldBookshelfName}/rename?newName={newBookshelfName}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, newBookshelfName);
                    Assert.Equal(3, content[0].Books.Length);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, newBookshelfName);
                    Assert.Equal(3, content[0].Books.Length);
                });
        }

        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "NEW books")]
        public async Task Test_RenameBookshelf_BookshelfAlreadyExists(string username, string oldBookshelfName,
            string newBookshelfName)
        {
            await Client.PostAsync($"/bookshelves/{newBookshelfName}/add", username);

            await CheckForError(
                () => Client.PostAsync($"/bookshelves/{newBookshelfName}/rename?newName={oldBookshelfName}",
                    username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.BookshelfAlreadyExists);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, b => b.Name == oldBookshelfName);
                    Assert.Contains(content, b => b.Name == newBookshelfName);
                });
        }

        [Theory]
        [InlineData("parent1", "parent2", "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
        public async Task Test_RenameBookshelf_BookshelfSameNameSameChildNameDiffParents(string username1,
            string username2, string bookshelfName1, string bookshelfName2)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.PostAsync($"/bookshelves/{bookshelfName1}/rename?newName={bookshelfName2}", username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
        }

        [Theory]
        [InlineData("parent1", "parent2", "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
        public async Task Test_DeleteBookshelf_BookshelfSameNameSameChildNameDiffParents(string username1,
            string username2, string bookshelfName1, string bookshelfName2)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.DeleteAsync($"/bookshelves/{bookshelfName1}/delete", username1),
                HttpStatusCode.OK,
                Assert.Empty);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username1),
                HttpStatusCode.OK,
                Assert.Empty);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "1560bea2-7dcd-4b87-a9d3-e89012262270",
            "Eric's books")]
        public async Task Test_RemoveBookshelf_SameParentDiffChildSameBookshelfName(string username, Guid childId1, Guid childId2, string bookshelfName)
        {
            await Client.PutAsync($"/children/{childId2}/select", username);
            await Client.PostAsync($"/bookshelves/{bookshelfName}/add", username);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.DeleteAsync($"/bookshelves/{bookshelfName}/delete", username),
                HttpStatusCode.OK,
                Assert.Empty);

            await Client.PutAsync($"/children/{childId1}/select", username);

            await CheckResponse<List<BookshelfPreviewResponseDTO>>(async () => await Client.GetAsync($"/bookshelves/all", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName);
                });
        }

        [Theory]
        [InlineData("parent5", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_EmptyBookshelf(string username, string bookshelfName, string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Single(content.Books);
                    Assert.Contains(content.Books, b => b.BookId == bookId);
                });
        }

        [Theory]
        [InlineData("parent4", "Madison's collection", "OL3368273W", "OL48763W")]
        public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookNotAlreadyInBookshelf(string username,
            string bookshelfName, string bookId, string existingBookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Contains(content.Books, b => b.BookId == bookId);
                    Assert.Contains(content.Books, b => b.BookId == existingBookId);
                });
        }

        [Theory]
        [InlineData("parent4", "Madison's collection", "OL48763W")]
        public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookAlreadyInBookshelf(string username,
            string bookshelfName, string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Single(content.Books);
                    Assert.Contains(content.Books, b => b.BookId == bookId);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL3368286W", "OL48763W")]
        public async Task Test_BookshelfInsert_Multiple_NeitherAlreadyExists(string username, Guid childId,
            string bookshelfName, string bookId1, string bookId2)
        {
            await Client.PutAsync($"/children/{childId}/select", username);
            
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId1}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(3, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId2}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(4, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL28633459W", "OL48763W")]
        public async Task Test_BookshelfInsert_Multiple_OneAlreadyExists(string username, Guid childId,
            string bookshelfName, string bookId1, string bookId2)
        {
            await Client.PutAsync($"/children/{childId}/select", username);

            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId1}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(2, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId2}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(3, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL28633459W", "OL3368273W")]
        public async Task Test_BookshelfInsert_Multiple_BothAlreadyExists(string username, Guid childId,
            string bookshelfName, string bookId1, string bookId2)
        {
            await Client.PutAsync($"/children/{childId}/select", username);

            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId1}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(2, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.PutAsync($"/bookshelves/{bookshelfName}/insert/{bookId2}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(2, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
        }

        [Theory]
        [InlineData("parent4", "Madison's collection", "OL48763W")]
        public async Task Test_RemoveBookshelfItem_SingleItemInBookshelf(string username, string bookshelfName,
            string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.DeleteAsync($"/bookshelves/{bookshelfName}/remove/{bookId}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Empty(content.Books);
                });
        }

        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL3368288W")]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL48763W")]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL28633459W")]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)", "OL3368286W")]
        public async Task Test_RemoveBookshelfItem_MultipleItemsInBookshelf(string username, string bookshelfName,
            string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.DeleteAsync($"/bookshelves/{bookshelfName}/remove/{bookId}", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(3, content.Books.Length);
                    Assert.DoesNotContain(content.Books, c => c.BookId == bookId);
                });
        }

        [Theory]
        [InlineData("parent1", "Evelyn's bookshelf (Parent1)")]
        public async Task Test_ClearBookshelf_Basic(string username, string bookshelfName)
        {
            await CheckResponse(async () => await Client.DeleteAsync($"/bookshelves/{bookshelfName}/clear", username), HttpStatusCode.NoContent);

            await CheckResponse<BookshelfPreviewResponseDTO>(async () => await Client.GetAsync($"/bookshelves/{bookshelfName}/details", username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Empty(content.Books);
                });
        }
    }
}