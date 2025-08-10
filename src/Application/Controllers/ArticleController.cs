using Application.Middlewares;
using Application.Service;
using Application.DTOs.Article;
using Microsoft.AspNetCore.Mvc;

namespace Application.Controllers;

/// <summary>
/// Controller for managing museum articles
/// </summary>
[Route("api/v1/articles")]
[ApiController]
[Produces("application/json")]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;

    public ArticleController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    /// <summary>
    /// Get a paginated list of published articles
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <returns>A list of published articles with pagination info</returns>
    /// <response code="200">Returns the list of articles</response>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ArticleQuery query)
    {
        return await _articleService.HandleGetAll(query);
    }

    /// <summary>
    /// Get a paginated list of all articles for admin purposes
    /// </summary>
    /// <param name="query">Admin query parameters for filtering and pagination</param>
    /// <returns>A list of all articles with pagination info</returns>
    /// <response code="200">Returns the list of articles</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] ArticleAdminQuery query)
    {
        return await _articleService.HandleGetAllAdmin(query);
    }

    /// <summary>
    /// Get a specific article by ID
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>Article details</returns>
    /// <response code="200">Returns the article</response>
    /// <response code="404">Article not found</response>
    /// <response code="401">Unauthorized - Authentication required for non-published articles</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _articleService.HandleGetById(id);
    }

    /// <summary>
    /// Create a new article
    /// </summary>
    /// <param name="createDto">Article creation data</param>
    /// <returns>Created article</returns>
    /// <response code="201">Article created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArticleCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return await _articleService.HandleCreate(createDto);
    }

    /// <summary>
    /// Update an existing article
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <param name="updateDto">Article update data</param>
    /// <returns>Updated article</returns>
    /// <response code="200">Article updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Article not found</response>
    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ArticleUpdateDto updateDto)
    {
        return await _articleService.HandleUpdate(id, updateDto);
    }

    /// <summary>
    /// Delete an article
    /// </summary>
    /// <param name="id">The article ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Article deleted successfully</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Article not found</response>
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _articleService.HandleDelete(id);
    }
}