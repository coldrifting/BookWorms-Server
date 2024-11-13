using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // So the Roles list can successfully be persisted and retrieved
        // (string arrays don't play nice with MySQL and EF)
        modelBuilder.Entity<User>()
            .Property(u => u.Roles)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries),
                new ValueComparer<string[]>(
                    (a1, a2) => a1!.SequenceEqual(a2!),
                    a => a.Aggregate(0, (i, v) => HashCode.Combine(i, v.GetHashCode()))));
    }
}