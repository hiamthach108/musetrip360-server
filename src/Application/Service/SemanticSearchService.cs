namespace Application.Service;

using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Core.Elasticsearch;
using Core.LLM;
using Application.DTOs.Search;
using Infrastructure.Repository;
using Database;
using MuseTrip360.src.Infrastructure.Repository;
using Application.Shared.Type;

public interface ISemanticSearchService
{
  Task<IActionResult> HandleSemanticSearchAsync(SemanticSearchQuery query);
  Task<IActionResult> GenerateEmbeddingAsync(EmbeddingRequestDto request);
  Task<IActionResult> GetSimilarItemsAsync(Guid itemId, string itemType, int size = 5);

  Task<SemanticSearchResultDto> SearchByQueryAsync(SemanticSearchQuery query);
  Task<bool> IndexItemWithEmbeddingAsync(SemanticSearchItemDto item);
  Task<bool> DeleteItemFromSemanticIndexAsync(Guid id);
  Task<bool> BulkIndexItemsWithEmbeddingsAsync(IEnumerable<SemanticSearchItemDto> items);
  Task<bool> CreateSemanticSearchIndexAsync();
  Task<bool> RecreateSemanticSearchIndexAsync();

  Task<bool> IndexMuseumSemanticAsync(Guid museumId);
  Task<bool> IndexArtifactSemanticAsync(Guid artifactId);
  Task<bool> IndexEventSemanticAsync(Guid eventId);
  Task<bool> IndexTourContentSemanticAsync(Guid tourContentId);
  Task<bool> IndexTourOnlineSemanticAsync(Guid tourOnlineId);
}

public class SemanticSearchService : BaseService, ISemanticSearchService
{
  private readonly IVectorSearchService _vectorSearchService;
  private readonly ILLM _llm;
  private readonly IMuseumRepository _museumRepository;
  private readonly IArtifactRepository _artifactRepository;
  private readonly IEventRepository _eventRepository;
  private readonly ITourContentRepository _tourContentRepository;
  private readonly ITourOnlineRepository _tourOnlineRepository;
  private readonly string _semanticSearchIndex = "semantic_search_items";
  private readonly int _vectorDimensions = 768;

  public SemanticSearchService(
    MuseTrip360DbContext dbContext,
    IVectorSearchService vectorSearchService,
    ILLM llm,
    IMapper mapper,
    IHttpContextAccessor httpCtx) : base(dbContext, mapper, httpCtx)
  {
    _vectorSearchService = vectorSearchService;
    _llm = llm;
    _museumRepository = new MuseumRepository(dbContext);
    _artifactRepository = new ArtifactRepository(dbContext);
    _eventRepository = new EventRepository(dbContext);
    _tourContentRepository = new TourContentRepository(dbContext);
    _tourOnlineRepository = new TourOnlineRepository(dbContext);
  }

  public async Task<IActionResult> HandleSemanticSearchAsync(SemanticSearchQuery query)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(query.Query))
      {
        return ErrorResp.BadRequest("Search query cannot be empty");
      }

      var response = await SearchByQueryAsync(query);
      if (response == null)
      {
        return ErrorResp.InternalServerError("Failed to perform semantic search");
      }

      return SuccessResp.Ok(response);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error occurred during semantic search: {ex.Message}");
    }
  }

  public async Task<IActionResult> GenerateEmbeddingAsync(EmbeddingRequestDto request)
  {
    try
    {
      if (string.IsNullOrWhiteSpace(request.Text))
      {
        return ErrorResp.BadRequest("Text cannot be empty");
      }

      var embedding = await _llm.EmbedAsync(request.Text);
      if (embedding == null || !embedding.Any())
      {
        return ErrorResp.InternalServerError("Failed to generate embedding");
      }

      var response = new EmbeddingResponseDto
      {
        Embedding = embedding.ToArray(),
        Dimensions = embedding.Count,
        ProcessedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
      };

      return SuccessResp.Ok(response);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error generating embedding: {ex.Message}");
    }
  }

  public async Task<IActionResult> GetSimilarItemsAsync(Guid itemId, string itemType, int size = 5)
  {
    try
    {
      string searchText = "";

      switch (itemType.ToLower())
      {
        case "museum":
          var museum = _museumRepository.GetById(itemId);
          if (museum == null) return ErrorResp.NotFound("Museum not found");
          searchText = $"{museum.Name} {museum.Description} {museum.Location}";
          break;

        case "artifact":
          var artifact = await _artifactRepository.GetByIdAsync(itemId);
          if (artifact == null) return ErrorResp.NotFound("Artifact not found");
          searchText = $"{artifact.Name} {artifact.Description}";
          break;

        case "event":
          var eventEntity = await _eventRepository.GetEventById(itemId);
          if (eventEntity == null) return ErrorResp.NotFound("Event not found");
          searchText = $"{eventEntity.Title} {eventEntity.Description} {eventEntity.Location}";
          break;

        case "tourcontent":
          var tourContent = await _tourContentRepository.GetTourContentById(itemId);
          if (tourContent == null) return ErrorResp.NotFound("Tour content not found");
          searchText = tourContent.Content;
          break;

        case "touronline":
          var tourOnline = await _tourOnlineRepository.GetByIdAsync(itemId);
          if (tourOnline == null) return ErrorResp.NotFound("Tour online not found");
          searchText = $"{tourOnline.Name} {tourOnline.Description}";
          break;

        default:
          return ErrorResp.BadRequest("Invalid item type");
      }

      if (string.IsNullOrWhiteSpace(searchText))
      {
        return ErrorResp.BadRequest("No searchable content found for the item");
      }

      var queryEmbedding = await _llm.EmbedAsync(searchText);
      if (queryEmbedding == null || !queryEmbedding.Any())
      {
        return ErrorResp.InternalServerError("Failed to generate embedding for similarity search");
      }

      var additionalFilter = $"NOT id:{itemId}";

      var (searchResults, totalHits, scores) = await _vectorSearchService.VectorSearchAsync<SemanticSearchItemDto>(
        _semanticSearchIndex,
        queryEmbedding.ToArray(),
        0,
        size,
        0.6f,
        additionalFilter
      );

      var items = searchResults.ToList();
      var scoresArray = scores.ToArray();

      for (int i = 0; i < items.Count && i < scoresArray.Length; i++)
      {
        items[i].SimilarityScore = scoresArray[i];
        items[i].Embedding = null;
      }

      return SuccessResp.Ok(new
      {
        SimilarItems = items,
        Total = items.Count,
        MaxSimilarityScore = scoresArray.Length > 0 ? scoresArray.Max() : 0f,
        MinSimilarityScore = scoresArray.Length > 0 ? scoresArray.Min() : 0f
      });
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error finding similar items: {ex.Message}");
    }
  }


  public async Task<SemanticSearchResultDto> SearchByQueryAsync(SemanticSearchQuery query)
  {
    if (string.IsNullOrWhiteSpace(query.Query))
    {
      throw new ArgumentException("Search query cannot be empty", nameof(query.Query));
    }

    var queryEmbedding = await _llm.EmbedAsync(query.Query);
    if (queryEmbedding == null || !queryEmbedding.Any())
    {
      throw new InvalidOperationException("Failed to generate embedding for search query");
    }

    var additionalFilter = BuildAdditionalFilter(query);
    var minScore = (float)(query.MinSimilarity ?? 0.7m);
    var from = (query.Page - 1) * query.PageSize;

    var (searchResults, totalHits, scores) = await _vectorSearchService.VectorSearchAsync<SemanticSearchItemDto>(
      _semanticSearchIndex,
      queryEmbedding.ToArray(),
      from,
      query.PageSize,
      minScore,
      additionalFilter
    );

    var items = searchResults.ToList();
    var scoresArray = scores.ToArray();

    for (int i = 0; i < items.Count && i < scoresArray.Length; i++)
    {
      items[i].SimilarityScore = scoresArray[i];
      if (!query.IncludeEmbeddings)
      {
        items[i].Embedding = null;
      }
    }

    var typeAggregations = items.GroupBy(x => x.Type)
      .ToDictionary(g => g.Key, g => g.Count());

    var response = new SemanticSearchResultDto
    {
      Items = items,
      Total = (int)totalHits,
      Page = query.Page,
      PageSize = query.PageSize,
      Query = query.Query,
      MaxSimilarityScore = scoresArray.Length > 0 ? scoresArray.Max() : 0f,
      MinSimilarityScore = scoresArray.Length > 0 ? scoresArray.Min() : 0f,
      TypeAggregations = typeAggregations
    };

    return response;
  }

  public async Task<bool> IndexItemWithEmbeddingAsync(SemanticSearchItemDto item)
  {
    try
    {
      var embedding = await _llm.EmbedAsync(item.SearchText);
      if (embedding == null || !embedding.Any())
      {
        return false;
      }

      return await _vectorSearchService.IndexDocumentWithVectorAsync(
        _semanticSearchIndex,
        item.Id.ToString(),
        item,
        embedding.ToArray()
      );
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> DeleteItemFromSemanticIndexAsync(Guid id)
  {
    try
    {
      return await _vectorSearchService.DeleteIndexAsync(id.ToString());
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> BulkIndexItemsWithEmbeddingsAsync(IEnumerable<SemanticSearchItemDto> items)
  {
    try
    {
      var itemsWithEmbeddings = new List<(SemanticSearchItemDto, float[])>();

      foreach (var item in items)
      {
        if (string.IsNullOrWhiteSpace(item.SearchText))
        {
          continue;
        }

        var embedding = await _llm.EmbedAsync(item.SearchText);
        if (embedding != null && embedding.Any())
        {
          itemsWithEmbeddings.Add((item, embedding.ToArray()));
        }

        await Task.Delay(100);
      }

      if (!itemsWithEmbeddings.Any())
      {
        return false;
      }

      return await _vectorSearchService.BulkIndexWithVectorsAsync(_semanticSearchIndex, itemsWithEmbeddings);
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> CreateSemanticSearchIndexAsync()
  {
    try
    {
      if (await _vectorSearchService.IndexExistsAsync(_semanticSearchIndex))
      {
        return true;
      }

      return await _vectorSearchService.CreateVectorIndexAsync(_semanticSearchIndex, _vectorDimensions);
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> RecreateSemanticSearchIndexAsync()
  {
    try
    {
      if (await _vectorSearchService.IndexExistsAsync(_semanticSearchIndex))
      {
        await _vectorSearchService.DeleteIndexAsync(_semanticSearchIndex);
      }

      var created = await _vectorSearchService.CreateVectorIndexAsync(_semanticSearchIndex, _vectorDimensions);

      if (created)
      {
        await BulkIndexAllSemanticItemsAsync();
      }

      return created;
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> IndexMuseumSemanticAsync(Guid museumId)
  {
    try
    {
      var museum = _museumRepository.GetById(museumId);
      if (museum == null) return false;

      var searchItem = _mapper.Map<SemanticSearchItemDto>(museum);
      return await IndexItemWithEmbeddingAsync(searchItem);
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> IndexArtifactSemanticAsync(Guid artifactId)
  {
    try
    {
      var artifact = await _artifactRepository.GetByIdAsync(artifactId);
      if (artifact == null) return false;

      var searchItem = _mapper.Map<SemanticSearchItemDto>(artifact);
      return await IndexItemWithEmbeddingAsync(searchItem);
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> IndexEventSemanticAsync(Guid eventId)
  {
    try
    {
      var eventEntity = await _eventRepository.GetEventById(eventId);
      if (eventEntity == null) return false;

      var searchItem = _mapper.Map<SemanticSearchItemDto>(eventEntity);
      return await IndexItemWithEmbeddingAsync(searchItem);
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> IndexTourContentSemanticAsync(Guid tourContentId)
  {
    try
    {
      var tourContent = await _tourContentRepository.GetTourContentById(tourContentId);
      if (tourContent == null) return false;

      var searchItem = _mapper.Map<SemanticSearchItemDto>(tourContent);
      return await IndexItemWithEmbeddingAsync(searchItem);
    }
    catch
    {
      return false;
    }
  }

  public async Task<bool> IndexTourOnlineSemanticAsync(Guid tourOnlineId)
  {
    try
    {
      var tourOnline = await _tourOnlineRepository.GetByIdAsync(tourOnlineId);
      if (tourOnline == null) return false;

      var searchItem = _mapper.Map<SemanticSearchItemDto>(tourOnline);
      return await IndexItemWithEmbeddingAsync(searchItem);
    }
    catch
    {
      return false;
    }
  }

  private async Task<bool> BulkIndexAllSemanticItemsAsync()
  {
    try
    {
      var allItems = new List<SemanticSearchItemDto>();

      var museums = _museumRepository.GetAllAdmin(new Application.DTOs.Museum.MuseumQuery { Page = 1, PageSize = int.MaxValue });
      foreach (var museum in museums.Museums)
      {
        var searchItem = _mapper.Map<SemanticSearchItemDto>(museum);
        allItems.Add(searchItem);
      }

      var artifacts = await _artifactRepository.GetAllAdminAsync(new MuseTrip360.src.Application.DTOs.Artifact.ArtifactAdminQuery { Page = 1, PageSize = int.MaxValue });
      foreach (var artifact in artifacts.Artifacts)
      {
        var searchItem = _mapper.Map<SemanticSearchItemDto>(artifact);
        allItems.Add(searchItem);
      }

      var events = await _eventRepository.GetAllAdminAsync(new EventAdminQuery { Page = 1, PageSize = int.MaxValue });
      foreach (var eventEntity in events.Events)
      {
        var searchItem = _mapper.Map<SemanticSearchItemDto>(eventEntity);
        allItems.Add(searchItem);
      }

      var tourContents = await _tourContentRepository.GetTourContentsAdmin(new TourContentAdminQuery { Page = 1, PageSize = int.MaxValue });
      foreach (var tourContent in tourContents.Contents)
      {
        var searchItem = _mapper.Map<SemanticSearchItemDto>(tourContent);
        allItems.Add(searchItem);
      }

      var tourOnlines = await _tourOnlineRepository.GetAllAdminAsync(new TourOnlineAdminQuery { Page = 1, PageSize = int.MaxValue });
      foreach (var tourOnline in tourOnlines.Tours)
      {
        var searchItem = _mapper.Map<SemanticSearchItemDto>(tourOnline);
        allItems.Add(searchItem);
      }

      return await BulkIndexItemsWithEmbeddingsAsync(allItems);
    }
    catch
    {
      return false;
    }
  }

  private string BuildAdditionalFilter(SemanticSearchQuery query)
  {
    var filterParts = new List<string>();

    if (!string.IsNullOrEmpty(query.Type))
    {
      filterParts.Add($"type:{query.Type}");
    }

    return filterParts.Count != 0 ? string.Join(" AND ", filterParts) : string.Empty;
  }
}