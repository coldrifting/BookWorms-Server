// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using WebApplication1.Models;
//
// namespace WebApplication1.Controllers;
//
// [ApiController]
// [Tags("Movie Examples")]
// [Route("api/[controller]/[action]")]
// public class ExampleController(BookwormsDbContext dbContext) : ControllerBase
// {
//     [HttpGet, Authorize]
//     [ProducesResponseType(StatusCodes.Status200OK)]
//     [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
//     public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesAuthenticated()
//     {
//         return await dbContext.Movies.ToListAsync();
//     }
//     
//     [HttpGet, Authorize(Roles="admin")]
//     public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesAuthorized()
//     {
//         return await dbContext.Movies.ToListAsync();
//     }
//     
//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
//     {
//         User.IsInRole("Administrators");
//         
//         return await dbContext.Movies.ToListAsync();
//     }
//
//     [HttpGet]
//     public async Task<ActionResult<Movie>> GetMovie(int id)
//     {
//         var movie = await dbContext.Movies.FindAsync(id);
//
//         if (movie == null)
//         {
//             return NotFound();
//         }
//
//         return movie;
//     }
// }