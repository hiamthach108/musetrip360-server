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

  public SearchController(ILogger<SearchController> logger, ISearchItemService searchItemService)
  {
    _logger = logger;
    _searchItemService = searchItemService;
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
}