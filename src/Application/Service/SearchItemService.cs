namespace Application.Service;

using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Core.Elasticsearch;
using Application.DTOs.Search;
using Infrastructure.Repository;
using Database;
using MuseTrip360.src.Infrastructure.Repository;
using Application.Shared.Type;


public interface ISearchItemService
{
  Task<IActionResult> HandleUnifiedSearchAsync(SearchQuery query);
  Task<IActionResult> HandleSearchSuggestionsAsync(string query, int size = 5);
  Task<IActionResult> HandleSearchAggregationsAsync();

  Task<bool> IndexItemAsync(SearchItemIndexDto item);
  Task<bool> DeleteItemFromIndexAsync(Guid id);
  Task<bool> BulkIndexItemsAsync(IEnumerable<SearchItemIndexDto> items);
  Task<bool> CreateSearchIndexAsync();
  Task<bool> RecreateSearchIndexAsync();

  Task<bool> IndexMuseumAsync(Guid museumId);
  Task<bool> IndexArtifactAsync(Guid artifactId);
  Task<bool> IndexEventAsync(Guid eventId);
  Task<bool> IndexTourContentAsync(Guid tourContentId);
  Task<bool> IndexTourOnlineAsync(Guid tourOnlineId);
  Task<bool> IndexTourGuideAsync(Guid tourGuideId);
}

public class SearchItemService : BaseService, ISearchItemService
{
  private readonly IElasticsearchService _elasticsearchService;
  private readonly IMuseumRepository _museumRepository;
  private readonly IArtifactRepository _artifactRepository;
  private readonly IEventRepository _eventRepository;
  private readonly ITourContentRepository _tourContentRepository;
  private readonly ITourOnlineRepository _tourOnlineRepository;
  private readonly ITourGuideRepository _tourGuideRepository;
  private readonly string _searchIndex = "search_items";

  public SearchItemService(
    MuseTrip360DbContext dbContext,
    IElasticsearchService elasticsearchService,
    IMapper mapper,
    IHttpContextAccessor httpCtx) : base(dbContext, mapper, httpCtx)
  {
    _elasticsearchService = elasticsearchService;
    _museumRepository = new MuseumRepository(dbContext);
    _artifactRepository = new ArtifactRepository(dbContext);
    _eventRepository = new EventRepository(dbContext);
    _tourContentRepository = new TourContentRepository(dbContext);
    _tourOnlineRepository = new TourOnlineRepository(dbContext);
    _tourGuideRepository = new TourGuideRepository(dbContext);
  }

  public async Task<IActionResult> HandleUnifiedSearchAsync(SearchQuery query)
  {
    try
    {
      var searchQuery = BuildSearchQuery(query);
      var from = (query.Page - 1) * query.PageSize;

      // Check if this is a geo distance search
      if (query.Latitude.HasValue && query.Longitude.HasValue && query.RadiusKm.HasValue)
      {
        var (searchResults, totalHits) = await _elasticsearchService.SearchWithGeoDistanceAsync<SearchItemIndexDto>(
          _searchIndex,
          searchQuery,
          query.Latitude.Value,
          query.Longitude.Value,
          query.RadiusKm.Value,
          from,
          query.PageSize
        );

        var items = searchResults.Select(item => _mapper.Map<SearchItem>(item)).ToList();

        var typeAggregations = searchResults.GroupBy(x => x.Type)
          .ToDictionary(g => g.Key, g => g.Count());

        var locationAggregations = searchResults
          .Where(x => !string.IsNullOrEmpty(x.Location))
          .GroupBy(x => x.Location!)
          .ToDictionary(g => g.Key, g => g.Count());

        var response = new SearchResultDto
        {
          Items = items,
          Total = (int)totalHits,
          Page = query.Page,
          PageSize = query.PageSize,
          TypeAggregations = typeAggregations,
          LocationAggregations = locationAggregations
        };

        return SuccessResp.Ok(response);
      }
      else
      {
        // Regular search without geo distance
        var (searchResults, totalHits) = await _elasticsearchService.SearchWithTotalAsync<SearchItemIndexDto>(
          _searchIndex,
          searchQuery,
          from,
          query.PageSize
        );

        var items = searchResults.Select(item => _mapper.Map<SearchItem>(item)).ToList();

        var typeAggregations = searchResults.GroupBy(x => x.Type)
          .ToDictionary(g => g.Key, g => g.Count());

        var locationAggregations = searchResults
          .Where(x => !string.IsNullOrEmpty(x.Location))
          .GroupBy(x => x.Location!)
          .ToDictionary(g => g.Key, g => g.Count());

        var response = new SearchResultDto
        {
          Items = items,
          Total = (int)totalHits,
          Page = query.Page,
          PageSize = query.PageSize,
          TypeAggregations = typeAggregations,
          LocationAggregations = locationAggregations
        };

        return SuccessResp.Ok(response);
      }
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError("Error occurred while performing unified search");
    }
  }

  public async Task<IActionResult> HandleSearchSuggestionsAsync(string query, int size = 5)
  {
    try
    {
      var searchQuery = $"searchText:*{query}* OR title:*{query}*";

      var searchResults = await _elasticsearchService.SearchAsync<SearchItemIndexDto>(
        _searchIndex,
        searchQuery,
        0,
        size
      );

      var suggestions = searchResults
        .GroupBy(x => x.Type)
        .Select(g => new SearchSuggestionDto
        {
          Text = g.First().Title,
          Type = g.Key,
          Count = g.Count()
        })
        .ToList();

      return SuccessResp.Ok(suggestions);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError("Error occurred while getting search suggestions");
    }
  }

  public async Task<IActionResult> HandleSearchAggregationsAsync()
  {
    try
    {
      var searchQuery = "*:*";
      var searchResults = await _elasticsearchService.SearchAsync<SearchItemIndexDto>(
        _searchIndex,
        searchQuery,
        0,
        1000
      );

      var aggregations = new
      {
        TotalItems = searchResults.Count(),
        ItemsByType = searchResults.GroupBy(x => x.Type)
          .ToDictionary(g => g.Key, g => g.Count()),
        ItemsByStatus = searchResults.GroupBy(x => x.Status)
          .ToDictionary(g => g.Key, g => g.Count()),
        ItemsByLocation = searchResults
          .Where(x => !string.IsNullOrEmpty(x.Location))
          .GroupBy(x => x.Location!)
          .ToDictionary(g => g.Key, g => g.Count()),
        AverageRating = searchResults
          .Where(x => x.Rating.HasValue)
          .Average(x => x.Rating ?? 0)
      };

      return SuccessResp.Ok(aggregations);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError("Error occurred while getting search aggregations");
    }
  }

  public async Task<bool> IndexItemAsync(SearchItemIndexDto item)
  {
    try
    {
      return await _elasticsearchService.IndexDocumentAsync(
        _searchIndex,
        item.Id.ToString(),
        item
      );
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> DeleteItemFromIndexAsync(Guid id)
  {
    try
    {
      return await _elasticsearchService.DeleteDocumentAsync(_searchIndex, id.ToString());
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> BulkIndexItemsAsync(IEnumerable<SearchItemIndexDto> items)
  {
    try
    {
      return await _elasticsearchService.BulkIndexAsync(_searchIndex, items);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> CreateSearchIndexAsync()
  {
    try
    {
      if (await _elasticsearchService.IndexExistsAsync(_searchIndex))
      {
        return true;
      }

      return await _elasticsearchService.CreateIndexAsync(_searchIndex);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> RecreateSearchIndexAsync()
  {
    try
    {
      if (await _elasticsearchService.IndexExistsAsync(_searchIndex))
      {
        await _elasticsearchService.DeleteIndexAsync(_searchIndex);
      }

      var created = await _elasticsearchService.CreateIndexAsync(_searchIndex);

      if (created)
      {
        await BulkIndexAllItemsAsync();
      }

      return created;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> IndexMuseumAsync(Guid museumId)
  {
    try
    {
      var museum = _museumRepository.GetById(museumId);
      if (museum == null) return false;

      var searchItem = _mapper.Map<SearchItemIndexDto>(museum);
      return await IndexItemAsync(searchItem);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> IndexArtifactAsync(Guid artifactId)
  {
    try
    {
      var artifact = await _artifactRepository.GetByIdAsync(artifactId);
      if (artifact == null) return false;

      var searchItem = _mapper.Map<SearchItemIndexDto>(artifact);
      return await IndexItemAsync(searchItem);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> IndexEventAsync(Guid eventId)
  {
    try
    {
      var eventEntity = await _eventRepository.GetEventById(eventId);
      if (eventEntity == null) return false;

      var searchItem = _mapper.Map<SearchItemIndexDto>(eventEntity);
      return await IndexItemAsync(searchItem);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> IndexTourContentAsync(Guid tourContentId)
  {
    try
    {
      var tourContent = await _tourContentRepository.GetTourContentById(tourContentId);
      if (tourContent == null) return false;

      var searchItem = _mapper.Map<SearchItemIndexDto>(tourContent);
      return await IndexItemAsync(searchItem);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> IndexTourOnlineAsync(Guid tourOnlineId)
  {
    try
    {
      var tourOnline = await _tourOnlineRepository.GetByIdAsync(tourOnlineId);
      if (tourOnline == null) return false;

      var searchItem = _mapper.Map<SearchItemIndexDto>(tourOnline);
      return await IndexItemAsync(searchItem);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public async Task<bool> IndexTourGuideAsync(Guid tourGuideId)
  {
    try
    {
      var tourGuide = await _tourGuideRepository.GetTourGuideByIdAsync(tourGuideId);
      if (tourGuide == null) return false;

      var searchItem = _mapper.Map<SearchItemIndexDto>(tourGuide);
      return await IndexItemAsync(searchItem);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  private async Task<bool> BulkIndexAllItemsAsync()
  {
    try
    {
      var allItems = new List<SearchItemIndexDto>();

      var museums = _museumRepository.GetAllAdmin(new Application.DTOs.Museum.MuseumQuery { Page = 1, PageSize = int.MaxValue });
      allItems.AddRange(museums.Museums.Select(m => _mapper.Map<SearchItemIndexDto>(m)));

      var artifacts = await _artifactRepository.GetAllAdminAsync(new MuseTrip360.src.Application.DTOs.Artifact.ArtifactAdminQuery { Page = 1, PageSize = int.MaxValue });
      allItems.AddRange(artifacts.Artifacts.Select(a => _mapper.Map<SearchItemIndexDto>(a)));

      var events = await _eventRepository.GetAllAdminAsync(new EventAdminQuery { Page = 1, PageSize = int.MaxValue });
      allItems.AddRange(events.Events.Select(e => _mapper.Map<SearchItemIndexDto>(e)));

      var tourContents = await _tourContentRepository.GetTourContentsAdmin(new TourContentAdminQuery { Page = 1, PageSize = int.MaxValue });
      allItems.AddRange(tourContents.Contents.Select(tc => _mapper.Map<SearchItemIndexDto>(tc)));

      var tourOnlines = await _tourOnlineRepository.GetAllAdminAsync(new TourOnlineAdminQuery { Page = 1, PageSize = int.MaxValue });
      allItems.AddRange(tourOnlines.Tours.Select(to => _mapper.Map<SearchItemIndexDto>(to)));

      var tourGuides = await _tourGuideRepository.GetAllTourGuidesAsync(new TourGuideQuery { Page = 1, PageSize = int.MaxValue });
      allItems.AddRange(tourGuides.TourGuides.Select(tg => _mapper.Map<SearchItemIndexDto>(tg)));

      return await BulkIndexItemsAsync(allItems);
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  private string BuildSearchQuery(SearchQuery query)
  {
    var queryParts = new List<string>();

    if (!string.IsNullOrEmpty(query.Search))
    {
      var searchTerm = query.Search.Trim();
      queryParts.Add($"(title:\"{searchTerm}\" OR title:*{searchTerm}* OR searchText:\"{searchTerm}\" OR searchText:*{searchTerm}*)");
    }

    if (!string.IsNullOrEmpty(query.Type))
    {
      queryParts.Add($"type:{query.Type}");
    }

    if (!string.IsNullOrEmpty(query.Location))
    {
      var locationTerm = query.Location.Trim();
      queryParts.Add($"(location:\"{locationTerm}\" OR location:*{locationTerm}*)");
    }

    if (!string.IsNullOrEmpty(query.Status))
    {
      queryParts.Add($"status:{query.Status}");
    }

    // Note: Geo distance queries need to be handled differently in Elasticsearch
    // The current Lucene query string syntax doesn't support geo distance queries
    // We need to use the Elasticsearch .NET client's geo distance query instead

    // If no query parts, return match all query
    if (!queryParts.Any())
    {
      return "*:*";
    }

    return string.Join(" AND ", queryParts);
  }
}