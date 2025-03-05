using System.Net;
using BookwormsServer.Models.Data;
using BookwormsServerTesting.Fixtures;
using BookwormsServerTesting.Helpers;
using Microsoft.EntityFrameworkCore;
using static BookwormsServerTesting.Helpers.Common;

namespace BookwormsServerTesting;

[Collection("Integration Tests")]
public class ClassroomTeacherTests(CompositeFixture fixture) : BookwormsIntegrationTests(fixture)
{
    [Fact]
    public async Task Test_TeacherClassroomRoutes_NotLoggedIn()
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.Classrooms.Details),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.Create("blah")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.Rename("blah")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.Delete),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfCreate("books")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfRename("books", "new books")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfDelete("books")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.BookshelfInsertBook("books", "bookId")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfRemoveBook("books", "bookId")),
            HttpStatusCode.Unauthorized,
            ErrorResponse.Unauthorized);
    }
    
    [Theory]
    [InlineData("admin")]
    [InlineData("parent1")]
    public async Task Test_TeacherClassroomRoutes_NotTeacher(string username)
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.Classrooms.Details, username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.Create("blah"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.Rename("blah"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.Delete, username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfCreate("books"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfRename("books", "new books"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfDelete("books"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.BookshelfInsertBook("books", "bookId"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfRemoveBook("books", "bookId"), username),
            HttpStatusCode.Forbidden,
            ErrorResponse.UserNotTeacher);
    }

    [Theory]
    [InlineData("teacher0")]
    public async Task Test_ClassDetails_NotExist(string username)
    {
        await CheckForError(
            async () => await Client.GetAsync(Routes.Classrooms.Details, username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }

    [Theory]
    [InlineData("teacher1")]
    public async Task Test_CreateClassroom_AlreadyExist(string username)
    {
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.Create("blah"), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorResponse.ClassroomAlreadyExists);
    }

    [Theory]
    [InlineData("teacher0", "new name")]
    public async Task Test_RenameClassroom_DoesNotExist(string username, string newClassName)
    {
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.Rename(newClassName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }

    [Theory]
    [InlineData("teacher0")]
    public async Task Test_DeleteClassroom_DoesNotExist(string username)
    {
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.Delete, username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }

    [Theory]
    [InlineData("teacher1", "Ms Johnson's Class", "ABC123", 2, 0, 1)]
    [InlineData("teacher3", "Utah History", "UTA801", 1, 2, 3)]
    public async Task Test_ClassDetails_Basic(string username, string className, string classCode, int numBooks, int numStudents, int classIcon)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.GetAsync(Routes.Classrooms.Details, username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(className, content.ClassroomName);
                Assert.Equal(classCode, content.ClassCode);
                Assert.Equal(classIcon, content.ClassIcon);
                Assert.Equal(numBooks, content.Bookshelves.Select(s => s.Books).Count());
                Assert.Equal(numStudents, content.Children.Count);
            });
    }

    [Theory]
    [InlineData("teacher0", "A New Class")]
    public async Task Test_CreateClass_Basic(string username, string className)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.PostAsync(Routes.Classrooms.Create(className), username),
            HttpStatusCode.Created,
            content =>
            {
                Assert.Equal(className, content.ClassroomName);
                Assert.NotNull(content.ClassCode);
                Assert.Equal(6, content.ClassCode.Length);
                Assert.Empty(content.Bookshelves.Select(s => s.Books));
                Assert.Empty(content.Children);
            });
        
        Assert.True(Context.Classrooms.Any(c => c.ClassroomName == className));
    }

    [Theory]
    [InlineData("teacher1", "Ms Johnson's Class", "New Rename Name")]
    public async Task Test_RenameClass_Basic(string username, string oldClassName, string renameName)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.PutAsync(Routes.Classrooms.Rename(renameName), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(renameName, content.ClassroomName);
                Assert.NotNull(content.ClassCode);
                Assert.Equal(6, content.ClassCode.Length);
            });
        
        Assert.True(Context.Classrooms.Any(c => c.ClassroomName == renameName));
        Assert.False(Context.Classrooms.Any(c => c.ClassroomName == oldClassName));
    }

    [Theory]
    [InlineData("teacher1")]
    public async Task Test_DeleteClass_Basic(string username)
    {
        await CheckResponse(
            async () => await Client.DeleteAsync(Routes.Classrooms.Delete, username),
            HttpStatusCode.NoContent);
        
        Assert.False(Context.Classrooms.Any(c => c.TeacherUsername == username));
    }

    [Theory]
    [InlineData("teacher2", "Class Books")]
    public async Task Test_CreateBookshelf_BookshelfAlreadyExist(string username, string bookshelfName)
    {
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfCreate(bookshelfName), username),
            HttpStatusCode.UnprocessableContent,
            ErrorResponse.BookshelfAlreadyExists);
        
        Assert.Single(Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves);
    }

    [Theory]
    [InlineData("teacher1", "Books 1", "Books A")]
    public async Task Test_RenameBookshelf_BookshelfAlreadyExist(string username, string bookshelfName, string newBookshelfName)
    {
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfRename(bookshelfName, newBookshelfName), username),
            HttpStatusCode.UnprocessableContent,
            ErrorResponse.BookshelfAlreadyExists);
        
        Assert.Equal(2, Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves.Count);
    }

    [Theory]
    [InlineData("teacher2", "Class Books 2")]
    public async Task Test_CreateBookshelf_Basic(string username, string bookshelfName)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfCreate(bookshelfName), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(2, content.Bookshelves.Count);
                Assert.Contains(bookshelfName, content.Bookshelves.Select(s => s.Name));
            });
        
        Assert.Equal(2, Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves.Count);
    }

    [Theory]
    [InlineData("teacher1", "Books 1", "Books B")]
    public async Task Test_RenameBookshelf_Basic(string username, string bookshelfName, string newBookshelfName)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfRename(bookshelfName, newBookshelfName), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(2, content.Bookshelves.Count);
                Assert.DoesNotContain(bookshelfName, content.Bookshelves.Select(s => s.Name));
                Assert.Contains(newBookshelfName, content.Bookshelves.Select(s => s.Name));
            });
        
        Assert.Equal(2, Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves.Count);
        Assert.DoesNotContain(bookshelfName, Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves.Select(s => s.Name));
    }

    [Theory]
    [InlineData("teacher0", "Books Not Found")]
    public async Task Test_ClassroomBookshelfOperations_ClassNotExist(string username, string bookshelfName)
    {
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfCreate(bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfRename(bookshelfName, "blah"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.BookshelfInsertBook(bookshelfName, "blah"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfRemoveBook(bookshelfName, "blah"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfDelete(bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.ClassroomNotFound);
    }

    [Theory]
    [InlineData("teacher1", "Books Not Found")]
    public async Task Test_ClassroomBookshelfOperations_BookshelfNotExist(string username, string bookshelfName)
    {
        await CheckForError(
            async () => await Client.PostAsync(Routes.Classrooms.BookshelfRename(bookshelfName, "blah"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
        
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.BookshelfInsertBook(bookshelfName, "blah"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfRemoveBook(bookshelfName, "blah"), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
        
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfDelete(bookshelfName), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfNotFound);
    }

    [Theory]
    [InlineData("teacher1", "Books A")]
    public async Task Test_DeleteBookshelf_Basic(string username, string bookshelfName)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfDelete(bookshelfName), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Single(content.Bookshelves);
                Assert.DoesNotContain(bookshelfName, content.Bookshelves.Select(s => s.Name));
            });
        
        Assert.Single(Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves);
        Assert.DoesNotContain(bookshelfName, Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves.Select(s => s.Name));
    }

    [Theory]
    [InlineData("teacher1", "Books 1", "OL47935W")]
    public async Task Test_RemoveBookshelfBook_Basic(string username, string bookshelfName, string bookId)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfRemoveBook(bookshelfName, bookId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(2, content.Bookshelves.Count);
                Assert.DoesNotContain(bookshelfName, content.Bookshelves
                    .First(b => b.Name == bookshelfName).Books
                    .Select(b => b.BookId));
            });
        
        Assert.Equal(2, Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves.Count);
        Assert.DoesNotContain(bookshelfName, Context.Classrooms
            .Include(c => c.Bookshelves)
            .ThenInclude(b => b.Books)
            .First(c => c.TeacherUsername == username).Bookshelves
            .First(b => b.Name == bookshelfName).Books
            .Select(b => b.BookId));
    }

    [Theory]
    [InlineData("teacher4", "Empty Readings", "BadId")]
    public async Task Test_InsertBookshelfBook_BadBookId(string username, string bookshelfName, string bookId)
    {
        await CheckForError(
            async () => await Client.PutAsync(Routes.Classrooms.BookshelfInsertBook(bookshelfName, bookId), username),
            HttpStatusCode.UnprocessableEntity,
            ErrorResponse.BookIdInvalid);
        
        Assert.Empty(Context.Classrooms
            .Include(c => c.Bookshelves)
            .ThenInclude(classroomBookshelf => classroomBookshelf.Books)
            .First(c => c.TeacherUsername == username).Bookshelves
            .First(b => b.Name == bookshelfName).Books);
    }

    [Theory]
    [InlineData("teacher4", "Empty Readings", "OL47935W")]
    public async Task Test_InsertBookshelfBook_Basic(string username, string bookshelfName, string bookId)
    {
        await CheckResponse<ClassroomTeacherResponse>(
            async () => await Client.PutAsync(Routes.Classrooms.BookshelfInsertBook(bookshelfName, bookId), username),
            HttpStatusCode.OK,
            content =>
            {
                Assert.Equal(2, content.Bookshelves.Count);
                Assert.Contains(bookId, content.Bookshelves
                    .First(b => b.Name == bookshelfName).Books
                    .Select(b => b.BookId));
            });
        
        Assert.Equal(2, Context.Classrooms
            .Include(c => c.Bookshelves)
            .First(c => c.TeacherUsername == username).Bookshelves.Count);
        Assert.Contains(bookId, Context.Classrooms
            .Include(c => c.Bookshelves)
            .ThenInclude(b => b.Books)
            .First(c => c.TeacherUsername == username).Bookshelves
            .First(b => b.Name == bookshelfName).Books
            .Select(b => b.BookId));
    }

    [Theory]
    [InlineData("teacher4", "Empty Readings", "OL892371W", 0)]
    [InlineData("teacher2", "Class Books", "OL47935W", 1)]
    [InlineData("teacher4", "History Readings", "OL3368288W", 2)]
    public async Task Test_RemoveBookshelfBook_NotInBookshelf(string username, string bookshelfName, string bookId, int remainingBooks)
    {
        await CheckForError(
            async () => await Client.DeleteAsync(Routes.Classrooms.BookshelfRemoveBook(bookshelfName, bookId), username),
            HttpStatusCode.NotFound,
            ErrorResponse.BookshelfBookNotFound);
        
        Assert.Equal(remainingBooks, Context.Classrooms
            .Include(c => c.Bookshelves)
            .ThenInclude(b => b.Books)
            .First(c => c.TeacherUsername == username).Bookshelves
            .First(b => b.Name == bookshelfName).Books.Count);
    }
}