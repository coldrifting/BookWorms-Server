using System.Text.Json;
using System.Text.Json.Serialization;
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
    public DbSet<DifficultyRating> DifficultyRatings { get; set; }
    
    public DbSet<Child> Children { get; set; }
    public DbSet<ChildBookshelf> ChildBookshelves { get; set; }
    public DbSet<ChildBookshelfBook> ChildBookshelfBooks { get; set; }
    public DbSet<CompletedBookshelf> CompletedBookshelves { get; set; }
    public DbSet<CompletedBookshelfBook> CompletedBookshelfBooks { get; set; }
    public DbSet<InProgressBookshelf> InProgressBookshelves { get; set; }
    public DbSet<InProgressBookshelfBook> InProgressBookshelfBooks { get; set; }
    
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<ClassroomChild> ClassroomChildren { get; set; }
    public DbSet<ClassroomBookshelf> ClassroomBookshelves { get; set; }
    public DbSet<ClassroomBookshelfBook> ClassroomBookshelfBooks { get; set; }
    
    public DbSet<ClassroomAnnouncement> ClassroomAnnouncements { get; set; }
    public DbSet<ClassroomAnnouncementsRead> ClassroomAnnouncementsRead { get; set; }

    public DbSet<Goal> Goals { get; set; }
    public DbSet<GoalChild> GoalChildren { get; set; }
    public DbSet<GoalClassBase> GoalClassesBase { get; set; }
    public DbSet<GoalClass> GoalClasses { get; set; }
    public DbSet<GoalClassAggregate> GoalClassAggregates { get; set; }
    public DbSet<GoalClassLog> GoalClassLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
	    // [CompletedBookshelf] M----M [Book]   using CompletedBookshelfBook
	    modelBuilder.Entity<CompletedBookshelf>()
		    .HasMany(completedBookshelf => completedBookshelf.Books)
		    .WithMany(book => book.CompletedBookshelves)
		    .UsingEntity<CompletedBookshelfBook>();
	    
	    modelBuilder.Entity<CompletedBookshelfBook>()
		    .HasOne(cbb => cbb.Bookshelf)
		    .WithMany()
		    .HasForeignKey(cbb => cbb.BookshelfId);
	    
	    modelBuilder.Entity<CompletedBookshelfBook>()
		    .HasOne(cbb => cbb.Book)
		    .WithMany()
		    .HasForeignKey(cbb => cbb.BookId);
	    
	    // [InProgressBookshelf] M----M [Book]   using InProgressBookshelfBook
	    modelBuilder.Entity<InProgressBookshelf>()
		    .HasMany(inProgressBookshelf => inProgressBookshelf.Books)
		    .WithMany(book => book.InProgressBookshelves)
		    .UsingEntity<InProgressBookshelfBook>();
	    
	    modelBuilder.Entity<InProgressBookshelfBook>()
		    .HasOne(ipbb => ipbb.Bookshelf)
		    .WithMany()
		    .HasForeignKey(ipbb => ipbb.BookshelfId);
	    
	    modelBuilder.Entity<InProgressBookshelfBook>()
		    .HasOne(ipbb => ipbb.Book)
		    .WithMany()
		    .HasForeignKey(ipbb => ipbb.BookId);
	    
	    // [ChildBookshelf] M----M [Book]   using ChildBookshelfBook
	    modelBuilder.Entity<ChildBookshelf>()
		    .HasMany(childBookshelf => childBookshelf.Books)
		    .WithMany(book => book.ChildBookshelves)
		    .UsingEntity<ChildBookshelfBook>();
	    
	    modelBuilder.Entity<ChildBookshelfBook>()
		    .HasOne(cbb => cbb.Bookshelf)
		    .WithMany()
		    .HasForeignKey(cbb => cbb.BookshelfId);
	    
	    modelBuilder.Entity<ChildBookshelfBook>()
		    .HasOne(cbb => cbb.Book)
		    .WithMany()
		    .HasForeignKey(cbb => cbb.BookId);

	    // [ClassroomBookshelf] M----M [Book]   using ClassroomBookshelfBook
	    modelBuilder.Entity<ClassroomBookshelf>()
		    .HasMany(classroomBookshelf => classroomBookshelf.Books)
		    .WithMany(book => book.ClassroomBookshelves)
		    .UsingEntity<ClassroomBookshelfBook>();
	    
	    modelBuilder.Entity<ClassroomBookshelfBook>()
		    .HasOne(cbb => cbb.Bookshelf)
		    .WithMany()
		    .HasForeignKey(cbb => cbb.BookshelfId);
	    
	    modelBuilder.Entity<ClassroomBookshelfBook>()
		    .HasOne(cbb => cbb.Book)
		    .WithMany()
		    .HasForeignKey(cbb => cbb.BookId);
	    
	    // [Child] M----M [Classroom]   using ClassroomChild
	    modelBuilder.Entity<Child>()
		    .HasMany(child => child.Classrooms)
		    .WithMany(classroom => classroom.Children)
		    .UsingEntity<ClassroomChild>();

	    modelBuilder.Entity<ClassroomChild>()
		    .HasMany(classroomChild => classroomChild.Announcements)
		    .WithMany(announce => announce.ChildrenRead)
		    .UsingEntity<ClassroomAnnouncementsRead>();
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
	    Seed<CompletedBookshelf>();
	    Seed<CompletedBookshelfBook>();
	    Seed<InProgressBookshelf>();
	    Seed<InProgressBookshelfBook>();
	    Seed<ChildBookshelf>();
	    Seed<ChildBookshelfBook>();

	    Seed<Classroom>();
	    Seed<ClassroomChild>();
	    Seed<ClassroomBookshelf>();
	    Seed<ClassroomBookshelfBook>();
	    Seed<ClassroomAnnouncement>();
	    
        Seed<GoalChild>();
        Seed<GoalClass>();
        Seed<GoalClassAggregate>();
        Seed<GoalClassLog>();
	    
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
	                           DELETE FROM Goals;
	                           DELETE FROM GoalClassLogs;
	                           DELETE FROM ClassroomAnnouncements;
	                           DELETE FROM ClassroomBookshelfBooks;
	                           DELETE FROM ClassroomBookshelves;
	                           DELETE FROM ClassroomChildren;
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