using System.Text.Json;
using System.Text.Json.Serialization;
using BookwormsServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer;

public class BookwormsDbContext(DbContextOptions<BookwormsDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Bookshelf> Bookshelves { get; set; }
    public DbSet<BookshelfBook> BookshelfBooks { get; set; }
    public DbSet<Child> Children { get; set; }
    public DbSet<ChildBookshelf> ChildBookshelves { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<ClassroomBookshelf> ClassroomBookshelves { get; set; }
    public DbSet<CompletedBookshelf> CompletedBookshelves { get; set; }
    public DbSet<InProgressBookshelf> InProgressBookshelves { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<User> Users { get; set; }
    
    public void SeedTestData()
    {
	    JsonSerializerOptions jso = new()
	    {
		    PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
	    };

	    // Database.EnsureDeleted();
        // Database.Migrate();
		Clear();
        
	    string userData = File.ReadAllText("TestData/UserEntities.json");
		var users = JsonSerializer.Deserialize<List<User>>(userData, jso)!;
		Users.AddRange(users);
		SaveChanges();
		
	    string parentData = File.ReadAllText("TestData/ParentEntities.json");
		var parents = JsonSerializer.Deserialize<List<Parent>>(parentData, jso)!;
		Parents.AddRange(parents);
		SaveChanges();

	    string teacherData = File.ReadAllText("TestData/TeacherEntities.json");
		var teachers = JsonSerializer.Deserialize<List<Teacher>>(teacherData, jso)!;
		Teachers.AddRange(teachers);
		SaveChanges();
		
	    string bookData = File.ReadAllText("TestData/BookEntities.json");
		var books = JsonSerializer.Deserialize<List<Book>>(bookData, jso)!;
		Books.AddRange(books);
		SaveChanges();
		
	    string reviewData = File.ReadAllText("TestData/ReviewEntities.json");
		var reviews = JsonSerializer.Deserialize<List<Review>>(reviewData, jso)!;
		Reviews.AddRange(reviews);
		SaveChanges();
		
	    string childData = File.ReadAllText("TestData/ChildEntities.json");
		var children = JsonSerializer.Deserialize<List<Child>>(childData, jso)!;
		Children.AddRange(children);
		SaveChanges();

		string childBookshelfData = File.ReadAllText("TestData/ChildBookshelfEntities.json");
		var childBookshelf = JsonSerializer.Deserialize<List<ChildBookshelf>>(childBookshelfData, jso)!;
		ChildBookshelves.AddRange(childBookshelf);
		SaveChanges();

		string bookshelfBookData = File.ReadAllText("TestData/BookshelfBookEntities.json");
		var bookshelfBooks = JsonSerializer.Deserialize<List<BookshelfBook>>(bookshelfBookData, jso)!;
		BookshelfBooks.AddRange(bookshelfBooks);
		SaveChanges();
		
		SaveChanges();
    }

    public void Clear()
    {
	    Database.ExecuteSqlRaw("DELETE FROM BookshelfBooks;");
	    Database.ExecuteSqlRaw("DELETE FROM Bookshelves;");
	    Database.ExecuteSqlRaw("DELETE FROM ChildBookshelves;");
	    Database.ExecuteSqlRaw("DELETE FROM Children;");
	    Database.ExecuteSqlRaw("DELETE FROM Reviews;");
	    Database.ExecuteSqlRaw("DELETE FROM Books;");
	    Database.ExecuteSqlRaw("DELETE FROM Users;");
    }
    
}