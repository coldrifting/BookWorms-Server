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
        [Theory]
        [InlineData("2a23200c-8fe0-4c8d-9233-3cf095569c01")]
        public async Task Test_GetAllBookshelves_AsLoggedOutShouldError(Guid childId)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Bookshelves.All(childId)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "2a23200c-8fe0-4c8d-9233-3cf095569c01")]
        public async Task Test_GetAllBookshelves_AsTeacherShouldError(string username, Guid childId)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Bookshelves.All(childId), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "2a23200c-8fe0-4c8d-9233-3cf095569c01")]
        public async Task Test_GetAllBookshelves_NoChildrenShouldError(string username, Guid childId)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Bookshelves.All(childId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("2a23200c-8fe0-4c8d-9233-3cf095569c01", "bookshelf")]
        public async Task Test_GetBookshelf_AsLoggedOutShouldError(Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "bookshelf")]
        public async Task Test_GetBookshelf_AsTeacherShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "00000000-0000-0000-0000-000000000000", "bookshelfName")]
        public async Task Test_GetBookshelf_NoChildrenShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "bookshelfName")]
        public async Task Test_GetBookshelf_BookshelfNotExist_ShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "bookshelf")]
        public async Task Test_AddBookshelf_AsLoggedOutShouldError(Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "00000000-0000-0000-0000-000000000000", "bookshelf")]
        public async Task Test_AddBookshelf_AsTeacherShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "00000000-0000-0000-0000-000000000000", "bookshelfName")]
        public async Task Test_AddBookshelf_NoChildrenShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)")]
        public async Task Test_AddBookshelf_BookshelfNameAlreadyExists(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.BookshelfAlreadyExists);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "bookshelf")]
        public async Task Test_RemoveBookshelf_AsLoggedOutShouldError(Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "00000000-0000-0000-0000-000000000000", "bookshelf")]
        public async Task Test_RemoveBookshelf_AsTeacherShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0",  "00000000-0000-0000-0000-000000000000", "bookshelfName")]
        public async Task Test_RemoveBookshelf_NoChildrenShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("00000000-0000-0000-0000-000000000000", "bookshelf", "newBookshelf")]
        public async Task Test_RenameBookshelf_AsLoggedOutShouldError(Guid childId, string bookshelfName, string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName)),
                HttpStatusCode.Unauthorized,
                ErrorDTO.Unauthorized);
        }

        [Theory]
        [InlineData("teacher1", "00000000-0000-0000-0000-000000000000", "bookshelf", "newBookshelf")]
        public async Task Test_RenameBookshelf_AsTeacherShouldError(string username, Guid childId, string bookshelfName, string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "00000000-0000-0000-0000-000000000000", "bookshelfName", "newBookshelf")]
        public async Task Test_RenameBookshelf_NoChildrenShouldError(string username, Guid childId, string bookshelfName, string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "invalidBookshelf", "newBookshelfName")]
        public async Task Test_RenameBookshelf_BookshelfNotExist(string username, Guid childId, string bookshelfName, string newBookshelfName)
        {
            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("teacher1", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_NotParent_ShouldError(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_NoChildren_ShouldError(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_BookshelfNameNotExist_ShouldError(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("parent5", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", "InvalidBookId")]
        public async Task Test_BookshelfInsert_BookIdNotValid_ShouldError(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.BookIdInvalid);
        }

        [Theory]
        [InlineData("teacher1", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfRemove_NotParent_ShouldError(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfRemove_NoChildren_ShouldError(string username, Guid childId, string bookshelfName,
            string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfDelete_BookshelfNotFound_ShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("parent2", "08dd3c4b-f197-4657-8556-58c76701802b", "Not a valid bookshelf", "OL3368273W")]
        public async Task Test_RemoveBookshelfItem_InvalidBookshelf(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }

        [Theory]
        [InlineData("parent5", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_RemoveBookshelfItem_EmptyBookshelf(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfBookNotFound);
        }

        [Theory]
        [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "Madison's collection", "invalidBookId")]
        public async Task Test_RemoveBookshelfItem_InvalidBookId(string username, Guid childId, string bookshelfName, string invalidBookId)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, invalidBookId), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfBookNotFound);
        }

        [Theory]
        [InlineData("teacher1", "00000000-0000-0000-0000-000000000000", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfClear_NotParent_ShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username),
                HttpStatusCode.Forbidden,
                ErrorDTO.UserNotParent);
        }

        [Theory]
        [InlineData("parent0", "00000000-0000-0000-0000-000000000000", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfClear_NoChildren_ShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.ChildNotFound);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Ryn's Empty Bookshelf")]
        public async Task Test_BookshelfClear_BookshelfNotFound_ShouldError(string username, Guid childId, string bookshelfName)
        {
            await CheckForError(
                () => Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username),
                HttpStatusCode.NotFound,
                ErrorDTO.BookshelfNotFound);
        }
        
        [Theory]
        [InlineData("parent5", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", 0)]
        [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "Madison's collection", 1)]
        [InlineData("parent2", "08dd3c4b-f197-4657-8556-58c76701802b", "Evelyn's bookshelf (Parent2)", 3)]
        public async Task Test_GetBookshelfDetails_Basic(string username, Guid childId, string bookshelfName, int expectedBooks)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(expectedBooks, content.Books.Length);
                    foreach (var b in content.Books)
                    {
                        Assert.NotNull(b.BookId);
                        Assert.NotEmpty(b.BookId);
                        Assert.NotNull(b.Author);
                        Assert.NotEmpty(b.BookId);
                        Assert.NotNull(b.Title);
                        Assert.NotEmpty(b.BookId);
                    }
                });
        }
        
        [Theory]
        [InlineData("parent5", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", 0)]
        [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "Madison's collection", 1)]
        [InlineData("parent2", "08dd3c4b-f197-4657-8556-58c76701802b", "Evelyn's bookshelf (Parent2)", 3)]
        public async Task Test_GetAllBookshelves_Basic(string username, Guid childId, string bookshelfName, int expectedBooks)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(bookshelfName, content[0].Name);
                    Assert.Equal(expectedBooks, content[0].Books.Length);
                    Assert.True(content[0].Books.Length <= 3, "Expected num of books per list to be less than 3");
                    foreach (var b in content[0].Books)
                    {
                        Assert.NotNull(b.BookId);
                        Assert.NotEmpty(b.BookId);
                        Assert.NotNull(b.Author);
                        Assert.NotEmpty(b.BookId);
                        Assert.NotNull(b.Title);
                        Assert.NotEmpty(b.BookId);
                    }
                });
        }
    }

    [Collection("Integration Tests")]
    public class ChildBookshelfWriteTests(AppFactory<Program> factory) : BaseTestWriteFixture(factory)
    {
        [Theory]
        [InlineData("parent2", "08dd3c4b-f197-4657-8556-58c76701802b", "Evelyn's bookshelf (Parent2)", 3)]
        public async Task Test_GetAllBookshelves_MultipleBookshelves(string username, Guid childId, string bookshelfName, int expectedBooks)
        {
            await Client.PostAsync(Routes.Bookshelves.Add(childId, "New"), username);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, c => c.Name == bookshelfName && c.Books.Length == 3);
                });
        }
        
        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "Evelyn_books")]
        public async Task Test_RenameBookshelf_BookshelfNameAlreadyExists(string username, Guid childId, string bookshelfName1,
            string bookshelfName2)
        {
            await Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName2), username);

            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName1, bookshelfName2), username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.BookshelfAlreadyExists);
        }

        [Theory]
        [InlineData("parent3", "1560bea2-7dcd-4b87-a9d3-e89012262270", "Books")]
        public async Task Test_AddBookshelf_Basic_ChildWithNoBookshelves(string username, Guid childId,
            string bookshelfName)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName);
                    Assert.Empty(content[0].Books);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName);
                    Assert.Empty(content[0].Books);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
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
            Guid child1Guid = await CheckResponse<List<ChildResponseDTO>, Guid>(
                async () => await Client.PostAsync(Routes.Children.Add(childName1), username1),
                HttpStatusCode.Created,
                (_, headers) => {
                    Guid? childGuid = headers.GetChildLocation();
                    Assert.NotNull(childGuid);
                    return childGuid.Value;
                });
            
            Guid child2Guid = await CheckResponse<List<ChildResponseDTO>, Guid>(
                async () => await Client.PostAsync(Routes.Children.Add(childName2), username2),
                HttpStatusCode.Created,
                (_, headers) => {
                    Guid? childGuid = headers.GetChildLocation();
                    Assert.NotNull(childGuid);
                    return childGuid.Value;
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.PostAsync(Routes.Bookshelves.Add(child1Guid, bookshelfName1), username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName1);
                    Assert.Empty(content[0].Books);
                });

            // Insert a book to distinguish the two 
            await Client.PutAsync(Routes.Bookshelves.Insert(child1Guid, bookshelfName1, bookId), username1);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.PostAsync(Routes.Bookshelves.Add(child2Guid, bookshelfName2), username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName2);
                    Assert.Empty(content[0].Books);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(child1Guid), username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, bookshelfName1);
                    Assert.Single(content[0].Books);
                    Assert.Contains(content[0].Books, c => c.BookId == bookId);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(child2Guid), username2),
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
        public async Task Test_DeleteBookshelf_BasicWithoutBooks(string username, Guid childId, string bookshelfName)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
                HttpStatusCode.OK,
                Assert.Empty);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)")]
        public async Task Test_DeleteBookshelf_BasicWithBooks(string username, Guid childId, string bookshelfName)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
                HttpStatusCode.OK,
                Assert.Empty);
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "bookshelf2")]
        public async Task Test_DeleteBookshelf_DeleteSingle_MultipleBookshelvesExistSameChild(string username, Guid childId, string existingBookshelf, string newBookshelf)
        {
            await Client.PostAsync(Routes.Bookshelves.Add(childId, newBookshelf), username);

            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(childId, existingBookshelf), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, newBookshelf);
                    Assert.Empty(content[0].Books);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "NEW books")]
        public async Task Test_RenameBookshelf_Basic(string username, Guid childId, string oldBookshelfName, string newBookshelfName)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.PostAsync(Routes.Bookshelves.Rename(childId, oldBookshelfName, newBookshelfName), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, newBookshelfName);
                    Assert.Equal(3, content[0].Books.Length);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.NotNull(content[0]);
                    Assert.Equal(content[0].Name, newBookshelfName);
                    Assert.Equal(3, content[0].Books.Length);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "NEW books")]
        public async Task Test_RenameBookshelf_BookshelfAlreadyExists(string username, Guid childId, string oldBookshelfName, string newBookshelfName)
        {
            await Client.PostAsync(Routes.Bookshelves.Add(childId, newBookshelfName), username);

            await CheckForError(
                () => Client.PostAsync(Routes.Bookshelves.Rename(childId, oldBookshelfName, newBookshelfName), username),
                HttpStatusCode.UnprocessableEntity,
                ErrorDTO.BookshelfAlreadyExists);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(2, content.Count);
                    Assert.Contains(content, b => b.Name == oldBookshelfName);
                    Assert.Contains(content, b => b.Name == newBookshelfName);
                });
        }

        [Theory]
        [InlineData("parent1", "parent2", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "08dd3c4b-f197-4657-8556-58c76701802b", "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
        public async Task Test_RenameBookshelf_BookshelfSameNameSameChildNameDiffParents(
            string username1, string username2, 
            Guid childId1, Guid childId2,
            string bookshelfName1, string bookshelfName2)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.PostAsync(Routes.Bookshelves.Rename(childId1, bookshelfName1, bookshelfName2), username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId1), username1),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId2), username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
        }

        [Theory]
        [InlineData("parent1", "parent2", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "08dd3c4b-f197-4657-8556-58c76701802b", "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
        public async Task Test_DeleteBookshelf_BookshelfSameNameSameChildNameDiffParents(
            string username1, string username2, 
            Guid childId1, Guid childId2, 
            string bookshelfName1, string bookshelfName2)
        {
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(childId1, bookshelfName1), username1),
                HttpStatusCode.OK,
                Assert.Empty);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId1), username1),
                HttpStatusCode.OK,
                Assert.Empty);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId2), username2),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName2);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "1560bea2-7dcd-4b87-a9d3-e89012262270", "Eric's books")]
        public async Task Test_RemoveBookshelf_SameParentDiffChildSameBookshelfName(string username, Guid childId1, Guid childId2, string bookshelfName)
        {
            await Client.PostAsync(Routes.Bookshelves.Add(childId2, bookshelfName), username);
            
            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(childId2, bookshelfName), username),
                HttpStatusCode.OK,
                Assert.Empty);

            await CheckResponse<List<BookshelfPreviewResponseDTO>>(
                async () => await Client.GetAsync(Routes.Bookshelves.All(childId1), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Single(content);
                    Assert.Contains(content, b => b.Name == bookshelfName);
                });
        }

        [Theory]
        [InlineData("parent5", "94706c0e-2be9-45b5-ab3d-b42d61ae6c47", "Ryn's Empty Bookshelf", "OL3368273W")]
        public async Task Test_BookshelfInsert_EmptyBookshelf(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Single(content.Books);
                    Assert.Contains(content.Books, b => b.BookId == bookId);
                });
        }

        [Theory]
        [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "Madison's collection", "OL3368273W", "OL48763W")]
        public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookNotAlreadyInBookshelf(string username, Guid childId, string bookshelfName, string bookId, string existingBookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Contains(content.Books, b => b.BookId == bookId);
                    Assert.Contains(content.Books, b => b.BookId == existingBookId);
                });
        }

        [Theory]
        [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "Madison's collection", "OL48763W")]
        public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookAlreadyInBookshelf(string username, Guid childId, string bookshelfName, string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
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
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId1), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(3, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId2), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(4, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL28633459W", "OL48763W", 2, 3)]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL48763W", "OL28633459W", 3, 3)]
        public async Task Test_BookshelfInsert_Multiple_OneAlreadyExists(string username, Guid childId,
            string bookshelfName, string bookId1, string bookId2, int expectedFirst, int expectedSecond)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId1), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(expectedFirst, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId2), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(expectedSecond, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
        }

        [Theory]
        [InlineData("parent3", "2a23200c-8fe0-4c8d-9233-3cf095569c01", "Eric's books", "OL28633459W", "OL3368273W")]
        public async Task Test_BookshelfInsert_Multiple_BothAlreadyExists(string username, Guid childId,
            string bookshelfName, string bookId1, string bookId2)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId1), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(2, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
            
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId2), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Equal(2, content.Books.Length);
                    Assert.Contains(content.Books, b => b.BookId == bookId1);
                    Assert.Contains(content.Books, b => b.BookId == bookId2);
                });
        }

        [Theory]
        [InlineData("parent4", "61a1be57-69a3-46d5-95cd-8e257d4a553c", "Madison's collection", "OL48763W")]
        public async Task Test_RemoveBookshelfItem_SingleItemInBookshelf(string username, Guid childId, string bookshelfName,
            string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Empty(content.Books);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "OL3368288W")]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "OL48763W")]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "OL28633459W")]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)", "OL3368286W")]
        public async Task Test_RemoveBookshelfItem_MultipleItemsInBookshelf(string username, Guid childId, string bookshelfName,
            string bookId)
        {
            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(3, content.Books.Length);
                    Assert.DoesNotContain(content.Books, c => c.BookId == bookId);
                });
        }

        [Theory]
        [InlineData("parent1", "ab198b2c-e08b-4f3d-a372-6af43c80e229", "Evelyn's bookshelf (Parent1)")]
        public async Task Test_ClearBookshelf_Basic(string username, Guid childId, string bookshelfName)
        {
            await CheckResponse(
                async () => await Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username), 
                HttpStatusCode.NoContent);

            await CheckResponse<BookshelfPreviewResponseDTO>(
                async () => await Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
                HttpStatusCode.OK,
                content => {
                    Assert.Equal(bookshelfName, content.Name);
                    Assert.Empty(content.Books);
                });
        }
    }
}