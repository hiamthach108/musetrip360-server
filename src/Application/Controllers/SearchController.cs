namespace Application.Controllers;

using System;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/search")]
public class SearchController : ControllerBase
{
  private readonly ILogger<SearchController> _logger;
  private readonly IMuseumSearchService _museumSearchService;

  public SearchController(ILogger<SearchController> logger, IMuseumSearchService museumSearchService)
  {
    _logger = logger;
    _museumSearchService = museumSearchService;
  }

  [HttpGet("museums")]
  public async Task<IActionResult> SearchMuseums([FromQuery] MuseumSearchQuery query)
  {
    _logger.LogInformation("Search museums request received with query: {Query}", query.Search);
    return await _museumSearchService.HandleSearchMuseumsAsync(query);
  }

  [HttpGet("museums/suggest")]
  public async Task<IActionResult> SuggestMuseums([FromQuery] string query, [FromQuery] int size = 5)
  {
    _logger.LogInformation("Museum suggestions request received with query: {Query}", query);
    return await _museumSearchService.HandleSuggestMuseumsAsync(query, size);
  }

  [HttpGet("museums/aggregations")]
  public async Task<IActionResult> GetMuseumAggregations()
  {
    _logger.LogInformation("Museum aggregations request received");
    return await _museumSearchService.HandleGetMuseumAggregationsAsync();
  }

  [HttpPost("museums/reindex")]
  public async Task<IActionResult> ReindexMuseums()
  {
    _logger.LogInformation("Museum reindex request received");
    var result = await _museumSearchService.RecreateMuseumIndexAsync();

    if (result)
    {
      return Ok(new { success = true, message = "Museums reindexed successfully" });
    }

    return BadRequest(new { success = false, message = "Failed to reindex museums" });
  }
}