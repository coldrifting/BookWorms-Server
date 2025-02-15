using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ChildBookshelfTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Theory]
    [InlineData(Constants.Parent3Child2Id)]
    public async Task Test_GetAllBookshelves_AsLoggedOutShouldError(string childId)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Bookshelves.All(childId)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }

    [Theory]
    [InlineData("teacher1", Constants.Parent3Child2Id)]
    public async Task Test_GetAllBookshelves_AsTeacherShouldError(string username, string childId)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Bookshelves.All(childId), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0", Constants.Parent3Child2Id)]
    public async Task Test_GetAllBookshelves_NoChildrenShouldError(string username, string childId)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Bookshelves.All(childId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData(Constants.Parent3Child2Id, "bookshelf")]
    public async Task Test_GetBookshelf_AsLoggedOutShouldError(string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }

    [Theory]
    [InlineData("teacher1", Constants.Parent3Child2Id, "bookshelf")]
    public async Task Test_GetBookshelf_AsTeacherShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0", Constants.EmptyChildId, "bookshelfName")]
    public async Task Test_GetBookshelf_NoChildrenShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "bookshelfName")]
    public async Task Test_GetBookshelf_BookshelfNotExist_ShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
    }

    [Theory]
    [InlineData(Constants.EmptyChildId, "bookshelf")]
    public async Task Test_AddBookshelf_AsLoggedOutShouldError(string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }

    [Theory]
    [InlineData("teacher1", Constants.EmptyChildId, "bookshelf")]
    public async Task Test_AddBookshelf_AsTeacherShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0", Constants.EmptyChildId, "bookshelfName")]
    public async Task Test_AddBookshelf_NoChildrenShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)")]
    public async Task Test_AddBookshelf_BookshelfNameAlreadyExists(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorResponse.BookshelfAlreadyExists);
    }

    [Theory]
    [InlineData(Constants.EmptyChildId, "bookshelf")]
    public async Task Test_RemoveBookshelf_AsLoggedOutShouldError(string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }

    [Theory]
    [InlineData("teacher1", Constants.EmptyChildId, "bookshelf")]
    public async Task Test_RemoveBookshelf_AsTeacherShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0",  Constants.EmptyChildId, "bookshelfName")]
    public async Task Test_RemoveBookshelf_NoChildrenShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData(Constants.EmptyChildId, "bookshelf", "newBookshelf")]
    public async Task Test_RenameBookshelf_AsLoggedOutShouldError(string childId, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName)),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }

    [Theory]
    [InlineData("teacher1", Constants.EmptyChildId, "bookshelf", "newBookshelf")]
    public async Task Test_RenameBookshelf_AsTeacherShouldError(string username, string childId, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0", Constants.EmptyChildId, "bookshelfName", "newBookshelf")]
    public async Task Test_RenameBookshelf_NoChildrenShouldError(string username, string childId, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "invalidBookshelf", "newBookshelfName")]
    public async Task Test_RenameBookshelf_BookshelfNotExist(string username, string childId, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName, newBookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
    }

    [Theory]
    [InlineData("teacher1", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_NotParent_ShouldError(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckForError(
            () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0", Constants.Parent1Child1Id, "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_NoChildren_ShouldError(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckForError(
            () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_BookshelfNameNotExist_ShouldError(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckForError(
            () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
    }

    [Theory]
    [InlineData("parent5", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", "InvalidBookId")]
    public async Task Test_BookshelfInsert_BookIdNotValid_ShouldError(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckForError(
            () => Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorResponse.BookIdInvalid);
    }

    [Theory]
    [InlineData("teacher1", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfRemove_NotParent_ShouldError(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfRemove_NoChildren_ShouldError(string username, string childId, string bookshelfName,
        string bookId)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfDelete_BookshelfNotFound_ShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
    }

    [Theory]
    [InlineData("parent2", Constants.Parent2Child1Id, "Not a valid bookshelf", "OL3368273W")]
    public async Task Test_RemoveBookshelfItem_InvalidBookshelf(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
    }

    [Theory]
    [InlineData("parent5", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_RemoveBookshelfItem_EmptyBookshelf(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfBookNotFound);
    }

    [Theory]
    [InlineData("parent4", Constants.Parent4Child1Id, "Madison's collection", "invalidBookId")]
    public async Task Test_RemoveBookshelfItem_InvalidBookId(string username, string childId, string bookshelfName, string invalidBookId)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, invalidBookId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfBookNotFound);
    }

    [Theory]
    [InlineData("teacher1", Constants.EmptyChildId, "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfClear_NotParent_ShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotParent);
    }

    [Theory]
    [InlineData("parent0", Constants.EmptyChildId, "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfClear_NoChildren_ShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ChildNotFound);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Ryn's Empty Bookshelf")]
    public async Task Test_BookshelfClear_BookshelfNotFound_ShouldError(string username, string childId, string bookshelfName)
    {
        await CheckForError(
            () => Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
    }
    
    [Theory]
    [InlineData("parent5", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", 0)]
    [InlineData("parent4", Constants.Parent4Child1Id, "Madison's collection", 1)]
    [InlineData("parent2", Constants.Parent2Child1Id, "Evelyn's bookshelf (Parent2)", 3)]
    public async Task Test_GetBookshelfDetails_Basic(string username, string childId, string bookshelfName, int expectedBooks)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.GetAsync(Routes.Bookshelves.Details(childId, bookshelfName), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Equal(expectedBooks, content.Books.Count);
                foreach (var b in content.Books)
                {
                    Assert.NotNull(b.BookId);
                    Assert.NotEmpty(b.BookId);
                    Assert.NotNull(b.Authors);
                    Assert.NotEmpty(b.Authors);
                    Assert.NotNull(b.Title);
                    Assert.NotEmpty(b.Title);
                }
            });
    }
    
    [Theory]
    [InlineData("parent5", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", 0)]
    [InlineData("parent4", Constants.Parent4Child1Id, "Madison's collection", 1)]
    [InlineData("parent2", Constants.Parent2Child1Id, "Evelyn's bookshelf (Parent2)", 3)]
    public async Task Test_GetAllBookshelves_Basic(string username, string childId, string bookshelfName, int expectedBooks)
    {
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.GetAsync(Routes.Bookshelves.All(childId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.NotNull(content[0]);
                Assert.Equal(bookshelfName, content[0].Name);
                Assert.Equal(expectedBooks, content[0].Books.Count);
                Assert.True(content[0].Books.Count <= 3, "Expected num of books per list to be less than 3");
                foreach (var b in content[0].Books)
                {
                    Assert.NotNull(b.BookId);
                    Assert.NotEmpty(b.BookId);
                    Assert.NotNull(b.Authors);
                    Assert.NotEmpty(b.Authors);
                    Assert.NotNull(b.Title);
                    Assert.NotEmpty(b.Title);
                }
            });
    }

    [Theory]
    [InlineData("parent2", Constants.Parent2Child1Id, "Evelyn's bookshelf (Parent2)", 3)]
    public async Task Test_GetAllBookshelves_MultipleBookshelves(string username, string childId, string bookshelfName, int expectedBooks)
    {
        Context.ChildBookshelves.Add(new("New", childId));
        await Context.SaveChangesAsync();
        
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.GetAsync(Routes.Bookshelves.All(childId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(2, content.Count);
                Assert.Contains(content, c => c.Name == bookshelfName && c.Books.Count == expectedBooks);
            });
    }
    
    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "Evelyn_books")]
    public async Task Test_RenameBookshelf_BookshelfNameAlreadyExists(string username, string childId, string bookshelfName1,
        string bookshelfName2)
    {
        Context.ChildBookshelves.Add(new(bookshelfName2, childId));
        await Context.SaveChangesAsync();

        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Rename(childId, bookshelfName1, bookshelfName2), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorResponse.BookshelfAlreadyExists);
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child1Id, "Books")]
    public async Task Test_AddBookshelf_Basic_ChildWithNoBookshelves(string username, string childId,
        string bookshelfName)
    {
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.PostAsync(Routes.Bookshelves.Add(childId, bookshelfName), username),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.NotNull(content[0]);
                Assert.Equal(content[0].Name, bookshelfName);
                Assert.Empty(content[0].Books);
            });

        List<ChildBookshelf> shelves = Context.ChildBookshelves
            .Where(c => c.ChildId == childId)
            .Include(childBookshelf => childBookshelf.Books).ToList(); 
        Assert.Single(shelves);
        Assert.NotNull(shelves[0]);
        Assert.Equal(shelves[0].Name, bookshelfName);
        Assert.Empty(shelves[0].Books);
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
        string child1Id = await CheckResponse<List<ChildResponse>, string>(
            async () => await Client.PostAsync(Routes.Children.Add(childName1), username1),
            HttpStatusCode.Created,
            (_, headers) => {
                string? childId = headers.GetChildLocation();
                Assert.NotNull(childId);
                return childId;
            });
        
        string child2Id = await CheckResponse<List<ChildResponse>, string>(
            async () => await Client.PostAsync(Routes.Children.Add(childName2), username2),
            HttpStatusCode.Created,
            (_, headers) => {
                string? childId = headers.GetChildLocation();
                Assert.NotNull(childId);
                return childId;
            });
        
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.PostAsync(Routes.Bookshelves.Add(child1Id, bookshelfName1), username1),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.NotNull(content[0]);
                Assert.Equal(content[0].Name, bookshelfName1);
                Assert.Empty(content[0].Books);
            });

        // Insert a book to distinguish the two 
        await Client.PutAsync(Routes.Bookshelves.Insert(child1Id, bookshelfName1, bookId), username1);
        
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.PostAsync(Routes.Bookshelves.Add(child2Id, bookshelfName2), username2),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.NotNull(content[0]);
                Assert.Equal(content[0].Name, bookshelfName2);
                Assert.Empty(content[0].Books);
            });
        
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.GetAsync(Routes.Bookshelves.All(child1Id), username1),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.NotNull(content[0]);
                Assert.Equal(content[0].Name, bookshelfName1);
                Assert.Single(content[0].Books);
                Assert.Contains(content[0].Books, c => c.BookId == bookId);
            });
        
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.GetAsync(Routes.Bookshelves.All(child2Id), username2),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.NotNull(content[0]);
                Assert.Equal(content[0].Name, bookshelfName2);
                Assert.Empty(content[0].Books);
            });
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child3Id, "Costanza's curation")]
    public async Task Test_DeleteBookshelf_BasicWithoutBooks(string username, string childId, string bookshelfName)
    {
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
            HttpStatusCode.OK,
            Assert.Empty);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)")]
    public async Task Test_DeleteBookshelf_BasicWithBooks(string username, string childId, string bookshelfName)
    {
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(childId, bookshelfName), username),
            HttpStatusCode.OK,
            Assert.Empty);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "bookshelf2")]
    public async Task Test_DeleteBookshelf_DeleteSingle_MultipleBookshelvesExistSameChild(string username, string childId, string existingBookshelf, string newBookshelf)
    {
        Context.ChildBookshelves.Add(new(newBookshelf, childId));
        await this.Context.SaveChangesAsync();

        await CheckResponse<List<BookshelfResponse>>(
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
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "NEW books")]
    public async Task Test_RenameBookshelf_Basic(string username, string childId, string oldBookshelfName, string newBookshelfName)
    {
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.PostAsync(Routes.Bookshelves.Rename(childId, oldBookshelfName, newBookshelfName), username),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.NotNull(content[0]);
                Assert.Equal(content[0].Name, newBookshelfName);
                Assert.Equal(3, content[0].Books.Count);
            });
        
        List<ChildBookshelf> shelves = Context.ChildBookshelves
            .Where(c => c.ChildId == childId)
            .Include(childBookshelf => childBookshelf.Books).ToList(); 
        Assert.Single(shelves);
        Assert.NotNull(shelves[0]);
        Assert.Equal(shelves[0].Name, newBookshelfName);
        Assert.Equal(4, shelves[0].Books.Count);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "NEW books")]
    public async Task Test_RenameBookshelf_BookshelfAlreadyExists(string username, string childId, string oldBookshelfName, string newBookshelfName)
    {
        Context.ChildBookshelves.Add(new(newBookshelfName, childId));
        await Context.SaveChangesAsync();

        await CheckForError(
            () => Client.PostAsync(Routes.Bookshelves.Rename(childId, oldBookshelfName, newBookshelfName), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorResponse.BookshelfAlreadyExists);
        
        List<ChildBookshelf> shelves = Context.ChildBookshelves
            .Where(c => c.ChildId == childId)
            .Include(childBookshelf => childBookshelf.Books).ToList(); 
        Assert.Equal(2, shelves.Count);
        Assert.Contains(shelves, b => b.Name == newBookshelfName);
        Assert.Contains(shelves, b => b.Name == oldBookshelfName);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, Constants.Parent2Child1Id, "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
    public async Task Test_RenameBookshelf_BookshelfSameNameSameChildNameDiffParents(
        string username1, string child1Id, string child2Id,
        string bookshelfName1, string bookshelfName2)
    {
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.PostAsync(Routes.Bookshelves.Rename(child1Id, bookshelfName1, bookshelfName2), username1),
            HttpStatusCode.OK,
            content => {
                Assert.Single(content);
                Assert.Contains(content, b => b.Name == bookshelfName2);
            });
        
        List<ChildBookshelf> child1Shelves = Context.ChildBookshelves
            .Where(c => c.ChildId == child1Id)
            .Include(childBookshelf => childBookshelf.Books).ToList();
        Assert.Single(child1Shelves);
        Assert.Contains(child1Shelves, b => b.Name == bookshelfName2);
        
        List<ChildBookshelf> child2Shelves = Context.ChildBookshelves
            .Where(c => c.ChildId == child2Id)
            .Include(childBookshelf => childBookshelf.Books).ToList();
        Assert.Single(child2Shelves);
        Assert.Contains(child2Shelves, b => b.Name == bookshelfName2);
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, Constants.Parent2Child1Id, "Evelyn's bookshelf (Parent1)", "Evelyn's bookshelf (Parent2)")]
    public async Task Test_DeleteBookshelf_BookshelfSameNameSameChildNameDiffParents(
        string username1,
        string child1Id, string child2Id,
        string bookshelfName1, string bookshelfName2)
    {
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(child1Id, bookshelfName1), username1),
            HttpStatusCode.OK,
            Assert.Empty);
        
        Assert.Empty(Context.ChildBookshelves.Where(c => c.ChildId == child1Id));
        
        List<ChildBookshelf> child2Shelves = Context.ChildBookshelves
            .Where(c => c.ChildId == child2Id)
            .Include(childBookshelf => childBookshelf.Books).ToList(); 
        Assert.Single(child2Shelves);
        Assert.Contains(child2Shelves, b => b.Name == bookshelfName2);
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child2Id, Constants.Parent3Child1Id, "Eric's books")]
    public async Task Test_RemoveBookshelf_SameParentDiffChildSameBookshelfName(string username, string child1Id, string child2Id, string bookshelfName)
    {
        Context.ChildBookshelves.Add(new(bookshelfName, child2Id));
        await Context.SaveChangesAsync();
        
        await CheckResponse<List<BookshelfResponse>>(
            async () => await Client.DeleteAsync(Routes.Bookshelves.Delete(child2Id, bookshelfName), username),
            HttpStatusCode.OK,
            Assert.Empty);

        List<ChildBookshelf> child1Shelves = Context.ChildBookshelves
            .Where(c => c.ChildId == child1Id)
            .Include(childBookshelf => childBookshelf.Books).ToList(); 
        Assert.Single(child1Shelves);
        Assert.Contains(child1Shelves, b => b.Name == bookshelfName);
    }

    [Theory]
    [InlineData("parent5", Constants.Parent5Child1Id, "Ryn's Empty Bookshelf", "OL3368273W")]
    public async Task Test_BookshelfInsert_EmptyBookshelf(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Single(content.Books);
                Assert.Contains(content.Books, b => b.BookId == bookId);
            });
    }

    [Theory]
    [InlineData("parent4", Constants.Parent4Child1Id, "Madison's collection", "OL3368273W", "OL48763W")]
    public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookNotAlreadyInBookshelf(string username, string childId, string bookshelfName, string bookId, string existingBookId)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Contains(content.Books, b => b.BookId == bookId);
                Assert.Contains(content.Books, b => b.BookId == existingBookId);
            });
    }

    [Theory]
    [InlineData("parent4", Constants.Parent4Child1Id, "Madison's collection", "OL48763W")]
    public async Task Test_BookshelfInsert_NonEmptyBookshelf_BookAlreadyInBookshelf(string username, string childId, string bookshelfName, string bookId)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Single(content.Books);
                Assert.Contains(content.Books, b => b.BookId == bookId);
            });
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child2Id, "Eric's books", "OL3368286W", "OL48763W")]
    public async Task Test_BookshelfInsert_Multiple_NeitherAlreadyExists(string username, string childId,
        string bookshelfName, string bookId1, string bookId2)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId1), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Equal(3, content.Books.Count);
                Assert.Contains(content.Books, b => b.BookId == bookId1);
            });
        
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId2), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Equal(4, content.Books.Count);
                Assert.Contains(content.Books, b => b.BookId == bookId1);
                Assert.Contains(content.Books, b => b.BookId == bookId2);
            });
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child2Id, "Eric's books", "OL28633459W", "OL48763W", 2, 3)]
    [InlineData("parent3", Constants.Parent3Child2Id, "Eric's books", "OL48763W", "OL28633459W", 3, 3)]
    public async Task Test_BookshelfInsert_Multiple_OneAlreadyExists(string username, string childId,
        string bookshelfName, string bookId1, string bookId2, int expectedFirst, int expectedSecond)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId1), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Equal(expectedFirst, content.Books.Count);
                Assert.Contains(content.Books, b => b.BookId == bookId1);
            });
        
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId2), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Equal(expectedSecond, content.Books.Count);
                Assert.Contains(content.Books, b => b.BookId == bookId1);
                Assert.Contains(content.Books, b => b.BookId == bookId2);
            });
    }

    [Theory]
    [InlineData("parent3", Constants.Parent3Child2Id, "Eric's books", "OL28633459W", "OL3368273W")]
    public async Task Test_BookshelfInsert_Multiple_BothAlreadyExists(string username, string childId,
        string bookshelfName, string bookId1, string bookId2)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId1), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Equal(2, content.Books.Count);
                Assert.Contains(content.Books, b => b.BookId == bookId1);
                Assert.Contains(content.Books, b => b.BookId == bookId2);
            });
        
        await CheckResponse<BookshelfResponse>(
            async () => await Client.PutAsync(Routes.Bookshelves.Insert(childId, bookshelfName, bookId2), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Equal(2, content.Books.Count);
                Assert.Contains(content.Books, b => b.BookId == bookId1);
                Assert.Contains(content.Books, b => b.BookId == bookId2);
            });
    }

    [Theory]
    [InlineData("parent4", Constants.Parent4Child1Id, "Madison's collection", "OL48763W")]
    public async Task Test_RemoveBookshelfItem_SingleItemInBookshelf(string username, string childId, string bookshelfName,
        string bookId)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(bookshelfName, content.Name);
                Assert.Empty(content.Books);
            });
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "OL3368288W")]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "OL48763W")]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "OL28633459W")]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)", "OL3368286W")]
    public async Task Test_RemoveBookshelfItem_MultipleItemsInBookshelf(string username, string childId, string bookshelfName,
        string bookId)
    {
        await CheckResponse<BookshelfResponse>(
            async () => await Client.DeleteAsync(Routes.Bookshelves.Remove(childId, bookshelfName, bookId), username),
            HttpStatusCode.OK,
            content => {
                Assert.Equal(3, content.Books.Count);
                Assert.DoesNotContain(content.Books, c => c.BookId == bookId);
            });
    }

    [Theory]
    [InlineData("parent1", Constants.Parent1Child1Id, "Evelyn's bookshelf (Parent1)")]
    public async Task Test_ClearBookshelf_Basic(string username, string childId, string bookshelfName)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.Bookshelves.Clear(childId, bookshelfName), username), 
            HttpStatusCode.NoContent);

        ChildBookshelf? shelf = Context.ChildBookshelves
            .Include(childBookshelf => childBookshelf.Books)
            .FirstOrDefault(cb => cb.Name == bookshelfName && cb.ChildId == childId);  
        Assert.NotNull(shelf);
        Assert.Empty(shelf.Books);
    }
}