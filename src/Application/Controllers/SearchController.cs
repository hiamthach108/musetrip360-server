namespace Application.Controllers;

using System;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Application.DTOs.Search;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/search")]
public class SearchController : ControllerBase
{
  private readonly ILogger<SearchController> _logger;
  private readonly ISearchItemService _searchItemService;
  private readonly ISemanticSearchService _semanticSearchService;

  public SearchController(ILogger<SearchController> logger, ISearchItemService searchItemService, ISemanticSearchService semanticSearchService)
  {
    _logger = logger;
    _searchItemService = searchItemService;
    _semanticSearchService = semanticSearchService;
  }

  [HttpGet]
  public async Task<IActionResult> UnifiedSearch([FromQuery] SearchQuery query)
  {

    return await _searchItemService.HandleUnifiedSearchAsync(query);
  }

  [HttpPost("reindex")]
  public async Task<IActionResult> RecreateIndex()
  {
    _logger.LogInformation("Search index recreation request received");
    var result = await _searchItemService.RecreateSearchIndexAsync();

    if (result)
    {
      return Ok(new { success = true, message = "Search index recreated successfully" });
    }

    return BadRequest(new { success = false, message = "Failed to recreate search index" });
  }

  [HttpPost("semantic")]
  public async Task<IActionResult> SemanticSearch([FromBody] SemanticSearchQuery query)
  {
    _logger.LogInformation("Semantic search request received for query: {Query}", query.Query);
    return await _semanticSearchService.HandleSemanticSearchAsync(query);
  }

  [HttpPost("embedding")]
  public async Task<IActionResult> GenerateEmbedding([FromBody] EmbeddingRequestDto request)
  {
    _logger.LogInformation("Embedding generation request received");
    return await _semanticSearchService.GenerateEmbeddingAsync(request);
  }

  [HttpGet("similar/{itemId}")]
  public async Task<IActionResult> GetSimilarItems(Guid itemId, [FromQuery] string itemType, [FromQuery] int size = 5)
  {
    _logger.LogInformation("Similar items request for {ItemType} with ID: {ItemId}", itemType, itemId);
    return await _semanticSearchService.GetSimilarItemsAsync(itemId, itemType, size);
  }

  [HttpPost("semantic/reindex")]
  public async Task<IActionResult> RecreateSemanticIndex()
  {
    _logger.LogInformation("Semantic search index recreation request received");
    var result = await _semanticSearchService.RecreateSemanticSearchIndexAsync();

    if (result)
    {
      return Ok(new { success = true, message = "Semantic search index recreated successfully" });
    }

    return BadRequest(new { success = false, message = "Failed to recreate semantic search index" });
  }

  [HttpPost("semantic/index/{itemType}/{itemId}")]
  public async Task<IActionResult> IndexItemSemantic(string itemType, Guid itemId)
  {
    _logger.LogInformation("Indexing {ItemType} with ID: {ItemId} for semantic search", itemType, itemId);
    
    bool result = itemType.ToLower() switch
    {
      "museum" => await _semanticSearchService.IndexMuseumSemanticAsync(itemId),
      "artifact" => await _semanticSearchService.IndexArtifactSemanticAsync(itemId),
      "event" => await _semanticSearchService.IndexEventSemanticAsync(itemId),
      "tourcontent" => await _semanticSearchService.IndexTourContentSemanticAsync(itemId),
      "touronline" => await _semanticSearchService.IndexTourOnlineSemanticAsync(itemId),
      _ => false
    };

    if (result)
    {
      return Ok(new { success = true, message = $"{itemType} indexed successfully for semantic search" });
    }

    return BadRequest(new { success = false, message = $"Failed to index {itemType} for semantic search" });
  }
}