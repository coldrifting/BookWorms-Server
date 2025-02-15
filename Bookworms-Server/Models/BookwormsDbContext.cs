using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookwormsServer;

public class BookwormsDbContext(DbContextOptions<BookwormsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    
    public DbSet<Book> Books { get; set; }
    public DbSet<Review> Reviews { get; set; }
    
    public DbSet<Child> Children { get; set; }
    public DbSet<ChildBookshelf> ChildBookshelves { get; set; }
    public DbSet<ChildBookshelfBook> ChildBookshelfBooks { get; set; }
    public DbSet<CompletedBookshelf> CompletedBookshelves { get; set; }
    public DbSet<CompletedBookshelfBook> CompletedBookshelfBooks { get; set; }
    public DbSet<InProgressBookshelf> InProgressBookshelves { get; set; }
    public DbSet<InProgressBookshelfBook> InProgressBookshelfBooks { get; set; }
    
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<ClassroomBookshelf> ClassroomBookshelves { get; set; }
    public DbSet<ClassroomBookshelfBook> ClassroomBookshelfBooks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
	    modelBuilder.Entity<CompletedBookshelf>()
		    .HasMany(cbb => cbb.Books)
		    .WithMany(b => b.CompletedBookshelves)
		    .UsingEntity<CompletedBookshelfBook>();
	    
	    modelBuilder.Entity<InProgressBookshelf>()
		    .HasMany(cbb => cbb.Books)
		    .WithMany(b => b.InProgressBookshelves)
		    .UsingEntity<InProgressBookshelfBook>();
	    
	    modelBuilder.Entity<ChildBookshelf>()
		    .HasMany(cbb => cbb.Books)
		    .WithMany(b => b.ChildBookshelves)
		    .UsingEntity<ChildBookshelfBook>();
	    
	    modelBuilder.Entity<ClassroomBookshelf>()
		    .HasMany(cbb => cbb.Books)
		    .WithMany(b => b.ClassroomBookshelves)
		    .UsingEntity<ClassroomBookshelfBook>();
    }
    
    private readonly JsonSerializerOptions _jso = new()
    {
	    PropertyNameCaseInsensitive = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
    
    public void SeedTestData()
    {
	    Clear();

	    Seed<Admin>();
	    Seed<Parent>();
	    Seed<Teacher>();
	    
	    Seed<Book>();
	    Seed<Review>();
		    
	    Seed<Child>();
	    Seed<ChildBookshelf>();
	    Seed<ChildBookshelfBook>();
	    
	    // Fix any inconsistencies that result from inserting directly into DB
	    foreach (Book book in Books.Include(b => b.Reviews))
	    {
			book.UpdateStarRating();
		    Books.Update(book);
	    }

	    SaveChanges();
    }

    public void Clear()
    {
	    Database.ExecuteSqlRaw("""
	                           SET FOREIGN_KEY_CHECKS = 0;
	                           DELETE FROM Books;
	                           DELETE FROM ChildBookshelfBooks;
	                           DELETE FROM ChildBookshelves;
	                           DELETE FROM Children;
	                           DELETE FROM ClassroomBookshelfBooks;
	                           DELETE FROM ClassroomBookshelves;
	                           DELETE FROM Classrooms;
	                           DELETE FROM CompletedBookshelfBooks;
	                           DELETE FROM CompletedBookshelves;
	                           DELETE FROM InProgressBookshelfBooks;
	                           DELETE FROM InProgressBookshelves;
	                           DELETE FROM Reviews;
	                           DELETE FROM Users;
	                           SET FOREIGN_KEY_CHECKS = 1;
	                           """);
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

public static class BookwormsDbContextExtensions
{
	/// <summary>
	/// Gets the current loged in user from the database.<para />
	/// Must be called from a controller class or method with the Authorize attribute
	/// </summary>
	/// <param name="users">The DB Set of Users</param>
	/// <param name="principal">The User Claim Priniciple from the validated token</param>
	/// <returns>The logged-in User Entity</returns>
	public static User CurrentUser(this DbSet<User> users, ClaimsPrincipal principal)
	{
		// Will not be null as long as this method is called from an authorized route
		string loggedInUsername = principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
		return users.Find(loggedInUsername)!;
	}

	public static Child? FindChild(this DbSet<Child> children, Parent parent, string childId)
	{
		Child? child = children.Find(childId);
		return child?.ParentUsername == parent.Username 
			? child 
			: null;
	}
}