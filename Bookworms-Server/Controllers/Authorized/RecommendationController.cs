using BookwormsServer.Models.Data;
using BookwormsServer.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookwormsServer.Controllers;

[Tags("Recommend")]
[Route("/Recommend/[action]")]
public class RecommendationController(BookwormsDbContext context, HttpClient httpClient): AuthControllerBase(context)
{
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
        User user = CurrentUser;
        string url = $"{endpoint}?username={user.Username}";
        if (!string.IsNullOrEmpty(childId))
        {
            if (CurrentUser is not Parent)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ErrorResponse.UserNotParent);
            }

            if (CurrentUserChild(childId) is not { })
            {
                return NotFound(ErrorResponse.ChildNotFound);
            }
            
            url += $"&childID={childId}";   
        }

        HttpResponseMessage response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            List<string> bookIds = await response.Content.ReadFromJsonAsync<List<string>>() ?? [];
            List<Book> books = DbContext.Books.Where(b => bookIds.Contains(b.BookId)).ToList();
            List<BookResponse> output = books.Select(book => book.ToResponse()).ToList();
            return Ok(output);
        }

        return Ok(new List<BookResponse>());
    }
}