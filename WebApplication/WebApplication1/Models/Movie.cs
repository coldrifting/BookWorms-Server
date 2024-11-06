using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models;

// An example of a model, which is automatically mapped to a DB via the context class
public class Movie
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Genre { get; set; }
    public DateTime ReleaseDate { get; set; }
}

public class MovieContext(DbContextOptions<MovieContext> options) : DbContext(options)
{
    public DbSet<Movie> Movies { get; set; }
}