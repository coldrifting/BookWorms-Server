using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ExampleController(MovieContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
    {
        return await dbContext.Movies.ToListAsync();
    }

    [HttpGet]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        var movie = await dbContext.Movies.FindAsync(id);

        if (movie == null)
        {
            return NotFound();
        }

        return movie;
    }
}