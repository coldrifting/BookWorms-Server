using System.Text.Json;
using System.Text.Json.Serialization;
using BookwormsServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer;

public class BookwormsDbContext(DbContextOptions<BookwormsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    
    public DbSet<Book> Books { get; set; }
    public DbSet<Review> Reviews { get; set; }
    
    public DbSet<Child> Children { get; set; }
    public DbSet<ChildBookshelf> ChildBookshelves { get; set; }
    public DbSet<Bookshelf> Bookshelves { get; set; }
    public DbSet<BookshelfBook> BookshelfBooks { get; set; }
    
    public DbSet<CompletedBookshelf> CompletedBookshelves { get; set; }
    public DbSet<InProgressBookshelf> InProgressBookshelves { get; set; }
    
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<ClassroomBookshelf> ClassroomBookshelves { get; set; }

    readonly JsonSerializerOptions _jso = new()
    {
	    PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public void SeedTestData()
    {
	    Clear();

	    Seed<User>();
	    Seed<Parent>();
	    Seed<Teacher>();
	    
	    Seed<Book>();
	    Seed<Review>();
		    
	    Seed<Child>();
	    Seed<ChildBookshelf>();
	    Seed<BookshelfBook>();
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
    
    private void Seed<T>() where T : class
    {
	    string typeName = typeof(T).Name;

	    string data = File.ReadAllText("TestData" + Path.DirectorySeparatorChar + typeName + "Entities.json");
		List<T>? objects = JsonSerializer.Deserialize<List<T>>(data, _jso);
		if (objects is null)
		{
			return;
		}

		Set<T>().AddRange(objects);
		SaveChanges();
    }
}