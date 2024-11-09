using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

public class RestDBContext(DbContextOptions<RestDBContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
}