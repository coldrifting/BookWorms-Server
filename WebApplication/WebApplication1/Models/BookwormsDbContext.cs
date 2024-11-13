using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public class BookwormsDbContext(DbContextOptions<BookwormsDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Child> Children { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Completed> Completeds { get; set; }
    public DbSet<Reading> Readings { get; set; }
}