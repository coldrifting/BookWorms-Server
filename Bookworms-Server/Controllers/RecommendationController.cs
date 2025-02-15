using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[ApiController]
[Authorize]
[Tags("Recommend")]
[Route("/Recommend/[action]")]
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
    /// <response code="404">The child ID is invalid, or is not managed by the logged-in user</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> SameAuthors(string? childId = null)
    {
        const string endpoint = "http://localhost:8000/recommend/same-authors";
        return await GetRecommendedBooks(endpoint, childId);
    }

    /// <summary>
    /// Gets a list of books with similar descriptions to other books the user has left positive reviews for
    /// </summary>
    /// <returns>A list of books with similar descriptions to other books the user has left positive reviews for</returns>
    /// <response code="200">Returns a list ofbooks with similar descriptions to other books the user has left positive reviews for</response>
    /// <response code="401">The user is not logged in</response>
    /// <response code="403">The user is not a parent</response>
    /// <response code="404">The child ID is invalid, or is not managed by the logged-in user</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<BookResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
    public async Task<IActionResult> SimilarDescriptions(string? childId = null)
    {
        const string endpoint = "http://localhost:8000/recommend/similar-descriptions";
        return await GetRecommendedBooks(endpoint, childId);
    }

    private async Task<IActionResult> GetRecommendedBooks(string endpoint, string? childId)
    {
        User user = dbContext.Users.CurrentUser(User);
        string url = $"{endpoint}?username={user.Username}";
        if (!string.IsNullOrEmpty(childId))
        {
            if (dbContext.Users.CurrentUser(User) is not Parent parent)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
            }

            if (dbContext.Children.FindChild(parent, childId) is not { } child)
            {
                return NotFound(ErrorResponse.ChildNotFound);
            }
            
            url += $"&childID={childId}";   
        }

        HttpResponseMessage response = await _client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            List<string> bookIds = await response.Content.ReadFromJsonAsync<List<string>>() ?? [];
            List<Book> books = dbContext.Books.Where(b => bookIds.Contains(b.BookId)).ToList();
            List<BookResponse> output = books.Select(book => book.ToResponse()).ToList();
            return Ok(output);
        }

        return Ok(new List<BookResponse>());
    }
}