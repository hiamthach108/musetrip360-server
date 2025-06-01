namespace Application.Service;

using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Core.Elasticsearch;
using Application.DTOs.Museum;
using Infrastructure.Repository;
using Database;
using Application.Shared.Type;
using Domain.Museums;
using Application.Shared.Enum;

public interface IMuseumSearchService
{
  Task<IActionResult> HandleSearchMuseumsAsync(MuseumSearchQuery query);
  Task<IActionResult> HandleSuggestMuseumsAsync(string query, int size = 5);
  Task<IActionResult> HandleGetMuseumAggregationsAsync();
  Task<bool> IndexMuseumAsync(Guid museumId);
  Task<bool> DeleteMuseumFromIndexAsync(Guid museumId);
  Task<bool> BulkIndexMuseumsAsync();
  Task<bool> CreateMuseumIndexAsync();
  Task<bool> RecreateMuseumIndexAsync();
}

public class MuseumSearchService : BaseService, IMuseumSearchService
{
  private readonly IElasticsearchService _elasticsearchService;
  private readonly IMuseumRepository _museumRepository;
  private readonly string _museumIndex = "museums";

  public MuseumSearchService(
    MuseTrip360DbContext dbContext,
    IElasticsearchService elasticsearchService,
    IMapper mapper,
    IHttpContextAccessor httpCtx) : base(dbContext, mapper, httpCtx)
  {
    _elasticsearchService = elasticsearchService;
    _museumRepository = new MuseumRepository(dbContext);
  }

  public async Task<IActionResult> HandleSearchMuseumsAsync(MuseumSearchQuery query)
  {
    try
    {
      query.Status = MuseumStatusEnum.Active;
      // Build search query
      var searchQuery = BuildSearchQuery(query);

      // Calculate pagination
      var from = (query.Page - 1) * query.PageSize;

      // Perform search
      var searchResults = await _elasticsearchService.SearchAsync<MuseumIndexDto>(
        _museumIndex,
        searchQuery,
        from,
        query.PageSize
      );

      // Map results to response DTOs
      var museums = searchResults.Select(m => _mapper.Map<MuseumDto>(m)).ToList();

      var response = new
      {
        List = museums,
        Total = museums.Count,
        Page = query.Page,
        PageSize = query.PageSize
      };

      return SuccessResp.Ok(response);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError("Error occurred while searching museums");
    }
  }

  public async Task<IActionResult> HandleSuggestMuseumsAsync(string query, int size = 5)
  {
    try
    {
      // Use simple prefix search for suggestions
      var searchQuery = $"name:{query}* OR description:{query}*";

      var searchResults = await _elasticsearchService.SearchAsync<MuseumIndexDto>(
        _museumIndex,
        searchQuery,
        0,
        size
      );

      var suggestions = searchResults.Select(m => new MuseumSuggestionDto
      {
        Id = m.Id,
        Name = m.Name,
        Location = m.Location,
        Rating = m.Rating
      }).ToList();

      return SuccessResp.Ok(suggestions);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError("Error occurred while getting suggestions");
    }
  }

  public async Task<IActionResult> HandleGetMuseumAggregationsAsync()
  {
    try
    {
      // Get museums from database for aggregations
      var museumQuery = new MuseumQuery { Page = 1, PageSize = int.MaxValue };
      var museums = _museumRepository.GetAll(museumQuery);

      var aggregations = new MuseumAggregationsDto
      {
        TotalMuseums = museums.Total,
        MuseumsByStatus = museums.Museums.GroupBy(m => m.Status)
          .ToDictionary(g => g.Key.ToString(), g => g.Count()),
        AverageRating = museums.Museums.Any() ? museums.Museums.Average(m => m.Rating) : 0,
        MuseumsByLocation = museums.Museums.GroupBy(m => m.Location)
          .ToDictionary(g => g.Key, g => g.Count())
      };

      return SuccessResp.Ok(aggregations);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError("Error occurred while getting aggregations");
    }
  }

  public async Task<bool> IndexMuseumAsync(Guid museumId)
  {
    try
    {
      var museum = _museumRepository.GetById(museumId);
      if (museum == null)
      {
        return false;
      }

      var museumIndexDto = _mapper.Map<MuseumIndexDto>(museum);

      museumIndexDto.Status = MuseumStatusEnum.Active;

      return await _elasticsearchService.IndexDocumentAsync(
        _museumIndex,
        museumId.ToString(),
        museumIndexDto
      );
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> DeleteMuseumFromIndexAsync(Guid museumId)
  {
    try
    {
      return await _elasticsearchService.DeleteDocumentAsync(_museumIndex, museumId.ToString());
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> BulkIndexMuseumsAsync()
  {
    try
    {
      var museumQuery = new MuseumQuery { Page = 1, PageSize = int.MaxValue };
      var museums = _museumRepository.GetAllAdmin(museumQuery);
      var museumIndexDtos = museums.Museums.Select(m => _mapper.Map<MuseumIndexDto>(m)).ToList();

      return await _elasticsearchService.BulkIndexAsync(_museumIndex, museumIndexDtos);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> CreateMuseumIndexAsync()
  {
    try
    {
      // Check if index already exists
      if (await _elasticsearchService.IndexExistsAsync(_museumIndex))
      {
        return true;
      }

      // Create index with Vietnamese-friendly settings
      return await _elasticsearchService.CreateIndexAsync(_museumIndex);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> RecreateMuseumIndexAsync()
  {
    try
    {
      // Delete existing index if exists
      if (await _elasticsearchService.IndexExistsAsync(_museumIndex))
      {
        await _elasticsearchService.DeleteIndexAsync(_museumIndex);
      }

      // Create new index with proper mapping
      var created = await _elasticsearchService.CreateIndexAsync(_museumIndex);

      if (created)
      {
        // Reindex all museums
        await BulkIndexMuseumsAsync();
      }

      return created;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  private string BuildSearchQuery(MuseumSearchQuery query)
  {
    var queryParts = new List<string>();

    // Full-text search with Vietnamese support
    if (!string.IsNullOrEmpty(query.Search))
    {
      // Use multi-match query for better Vietnamese text search
      var searchTerm = query.Search.Trim();
      queryParts.Add($"(name:\"{searchTerm}\" OR name:*{searchTerm}* OR description:\"{searchTerm}\" OR description:*{searchTerm}*)");
    }

    // Location filter with Vietnamese support
    if (!string.IsNullOrEmpty(query.Location))
    {
      var locationTerm = query.Location.Trim();
      queryParts.Add($"(location:\"{locationTerm}\" OR location:*{locationTerm}*)");
    }

    // Status filter
    if (query.Status.HasValue)
    {
      queryParts.Add($"status:{query.Status.ToString()}");
    }

    // Rating filter
    if (query.MinRating.HasValue)
    {
      queryParts.Add($"rating:[{query.MinRating.Value} TO *]");
    }

    // If no specific query, return all
    if (!queryParts.Any())
    {
      return "*:*";
    }

    return string.Join(" AND ", queryParts);
  }
}