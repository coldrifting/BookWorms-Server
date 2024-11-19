using Microsoft.EntityFrameworkCore;
using BookwormsServer.Models.Entities;

namespace BookwormsServer;

public class AllBookwormsDbContext(DbContextOptions<AllBookwormsDbContext> options) : DbContext(options)
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
}