using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Entities;

namespace WebApplication1.Models;

public class AllBookwormsDbContext(DbContextOptions<AllBookwormsDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Bookshelf> Bookshelfs { get; set; }
    public DbSet<BookshelfBook> BookshelfBooks { get; set; }
    public DbSet<BookshelfChild> BookshelfChildren { get; set; }
    public DbSet<BookshelfClassroom> BookshelfClassrooms { get; set; }
    public DbSet<Child> Children { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Completed> Completeds { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Reading> Readings { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<User> Users { get; set; }
}