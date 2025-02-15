using System.Security.Claims;
using BookwormsServer;
using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

[ApiController]
[Tags("Recommend")]
public class RecommendationController(BookwormsDbContext dbContext, HttpClient httpClient): ControllerBase
{
    private readonly HttpClient _client = httpClient;

    /// <summary>
    /// Gets a list of books by the same authors the user has left positive reviews for
    /// </summary>
    /// <returns>A list of books by the same authors the user has left positive reviews for</returns>
    /// <response code="200">Returns a list of books by the same authors the user has left positive reviews for</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child ID is invalid, or is not managed by the logged in user</response>
    [HttpGet]
    [Authorize]
    [Route("/Recommend/SameAuthors")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public async Task<IActionResult> RecommendSameAuthors(string? childId = null)
    {
        string endpoint = "http://localhost:8000/recommend/same-authors";
        return await GetRecommendedBooks(endpoint, childId);
    }

    /// <summary>
    /// Gets a list of books with similar descriptions to other books the user has left positive reviews for
    /// </summary>
    /// <returns>A list of books with similar descriptions to other books the user has left positive reviews for</returns>
    /// <response code="200">Returns a list ofbooks with similar descriptions to other books the user has left positive reviews for</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child ID is invalid, or is not managed by the logged in user</response>
    [HttpGet]
    [Authorize]
    [Route("/Recommend/SimilarDescriptions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookDTO>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorDTO))]
    public async Task<IActionResult> RecommendSimilarDescriptions(string? childId = null)
    {
        string endpoint = "http://localhost:8000/recommend/similar-descriptions";
        return await GetRecommendedBooks(endpoint, childId);
    }

    private async Task<IActionResult> GetRecommendedBooks(string endpoint, string? childId)
    {
        string loggedInUsername = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string url = $"{endpoint}?username={loggedInUsername}";
        if (!string.IsNullOrEmpty(childId))
        {
            if (!dbContext.Parents.Any(p => p.Username == loggedInUsername))
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorDTO.UserNotParent);
            }
            Child? child = dbContext.Children.Find(childId);
            if (child is null || child.ParentUsername != loggedInUsername)
            {
                return NotFound(ErrorDTO.ChildNotFound);
            }
            url += $"&childID={childId}";   
        }

        var response = await _client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            string content = await response.Content.ReadAsStringAsync();
            JArray jsonArray = JArray.Parse(content);
            List<string> bookIds = jsonArray?.ToObject<List<string>>() ?? [];
            List<Book> books = dbContext.Books.Where(b => bookIds.Contains(b.BookId)).ToList();
            List<BookDTO> output = books.Select(BookDTO.From).ToList();
            return Ok(output);
        }
        else
        {
            return Ok(new List<BookDTO>());
        }
    }
}