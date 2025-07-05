# SearchItem Elasticsearch Index Implementation Plan

## Overview
This plan outlines the implementation of a unified search index using the `SearchItem` DTO for searching across multiple entity types (Museums, Artifacts, Events, Tours) in MuseTrip360.

## Current SearchItem DTO Structure
```csharp
public class SearchItem
{
  public Guid Id { get; set; }
  public string Title { get; set; } = "";
  public string Type { get; set; } = "";
  public string? Thumbnail { get; set; }
  public string? Description { get; set; }
  public float? Lat { get; set; }
  public float? Lng { get; set; }
}
```

## Architecture Overview

### 1. Unified Search Index Strategy
- **Index Name**: `search_items`
- **Purpose**: Provide unified search across all searchable entities
- **Entity Types**: Museums, Artifacts, Events, TourContent, TourOnline, TourGuide

### 2. SearchItem Enhanced Structure
The current `SearchItem` DTO will be enhanced with additional fields for better search functionality:

```csharp
public class SearchItemIndexDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = "";
  public string Type { get; set; } = "";
  public string? Thumbnail { get; set; }
  public string? Description { get; set; }
  public float? Lat { get; set; }
  public float? Lng { get; set; }
  
  // Enhanced fields for better search
  public string? Location { get; set; }
  public double? Rating { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string Status { get; set; } = "";
  public Guid? RelatedEntityId { get; set; } // Museum ID for artifacts, events, etc.
  public Dictionary<string, object>? Metadata { get; set; }
  
  // Search optimization fields
  public string SearchText { get; set; } = ""; // Combined searchable text
  public string[] Tags { get; set; } = Array.Empty<string>();
}
```

## Implementation Plan

### Phase 1: Enhance SearchItem DTO and Create Supporting DTOs

#### 1.1 Update SearchItem.cs
- Add enhanced fields for better search functionality
- Maintain backward compatibility with existing structure

#### 1.2 Create SearchQuery.cs
```csharp
public class SearchQuery : PaginationReq
{
  public string? Query { get; set; }
  public string? Type { get; set; }
  public string? Location { get; set; }
  public double? MinRating { get; set; }
  public double? MaxRating { get; set; }
  public float? Lat { get; set; }
  public float? Lng { get; set; }
  public double? RadiusKm { get; set; }
  public string? Status { get; set; }
  public DateTime? StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public string? SortBy { get; set; }
  public string? SortOrder { get; set; } = "asc";
}
```

#### 1.3 Create Search Response DTOs
```csharp
public class SearchResultDto
{
  public List<SearchItem> Items { get; set; } = new();
  public int Total { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
  public Dictionary<string, int> TypeAggregations { get; set; } = new();
  public Dictionary<string, int> LocationAggregations { get; set; } = new();
}

public class SearchSuggestionDto
{
  public string Text { get; set; } = "";
  public string Type { get; set; } = "";
  public int Count { get; set; }
}
```

### Phase 2: Create SearchItemService

#### 2.1 Interface Definition
```csharp
public interface ISearchItemService
{
  Task<IActionResult> HandleUnifiedSearchAsync(SearchQuery query);
  Task<IActionResult> HandleSearchSuggestionsAsync(string query, int size = 5);
  Task<IActionResult> HandleSearchAggregationsAsync();
  
  // Index management
  Task<bool> IndexItemAsync(SearchItemIndexDto item);
  Task<bool> DeleteItemFromIndexAsync(Guid id);
  Task<bool> BulkIndexItemsAsync(IEnumerable<SearchItemIndexDto> items);
  Task<bool> CreateSearchIndexAsync();
  Task<bool> RecreateSearchIndexAsync();
  
  // Entity-specific indexing
  Task<bool> IndexMuseumAsync(Guid museumId);
  Task<bool> IndexArtifactAsync(Guid artifactId);
  Task<bool> IndexEventAsync(Guid eventId);
  Task<bool> IndexTourContentAsync(Guid tourContentId);
  Task<bool> IndexTourOnlineAsync(Guid tourOnlineId);
  Task<bool> IndexTourGuideAsync(Guid tourGuideId);
}
```

#### 2.2 Service Implementation Features
- Unified search across all entity types
- Geographic search with radius filtering
- Advanced filtering by type, status, rating, date ranges
- Auto-complete suggestions
- Search analytics and aggregations
- Real-time indexing when entities are created/updated

### Phase 3: Update SearchController

#### 3.1 Enhanced Controller Endpoints
```csharp
[ApiController]
[Route("/api/v1/search")]
public class SearchController : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> UnifiedSearch([FromQuery] SearchQuery query);
  
  [HttpGet("suggest")]
  public async Task<IActionResult> GetSuggestions([FromQuery] string query, [FromQuery] int size = 5);
  
  [HttpGet("aggregations")]
  public async Task<IActionResult> GetAggregations();
  
  [HttpPost("reindex")]
  public async Task<IActionResult> RecreateIndex();
  
  // Keep existing museum-specific endpoints for backward compatibility
  [HttpGet("museums")]
  public async Task<IActionResult> SearchMuseums([FromQuery] MuseumSearchQuery query);
}
```

### Phase 4: Entity Integration

#### 4.1 Mapping Profiles
Create AutoMapper profiles to convert domain entities to SearchItemIndexDto:

```csharp
public class SearchItemProfile : Profile
{
  public SearchItemProfile()
  {
    // Museum to SearchItem
    CreateMap<Museum, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Museum"))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description} {src.Location}"))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    
    // Artifact to SearchItem
    CreateMap<Artifact, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Artifact"))
      .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.MuseumId))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description}"));
    
    // Event to SearchItem
    CreateMap<Event, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Event"))
      .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.MuseumId))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description}"));
  }
}
```

#### 4.2 Service Integration
Update existing services to trigger indexing when entities are created/updated:

```csharp
// In MuseumService.cs
public async Task<IActionResult> HandleCreateAsync(MuseumCreateDto dto)
{
  // ... existing code ...
  
  // Index in unified search
  await _searchItemService.IndexMuseumAsync(museum.Id);
  
  return SuccessResp.Created(result);
}
```

### Phase 5: Advanced Search Features

#### 5.1 Geographic Search
- Implement radius-based location search using Elasticsearch geo-distance queries
- Support for "near me" functionality using coordinates

#### 5.2 Full-Text Search with Vietnamese Support
- Enhanced Vietnamese text analysis with custom analyzers
- Fuzzy search for typo tolerance
- Phrase matching and proximity search

#### 5.3 Faceted Search
- Dynamic facets based on entity types
- Location-based facets
- Rating range facets
- Date range facets

#### 5.4 Auto-Complete and Suggestions
- Real-time search suggestions
- Popular search terms
- Typo correction suggestions

### Phase 6: Performance Optimization

#### 6.1 Indexing Strategy
- Batch indexing for bulk operations
- Incremental indexing for real-time updates
- Background reindexing jobs

#### 6.2 Caching Strategy
- Cache frequent search queries using Redis
- Cache aggregation results
- Cache auto-complete suggestions

#### 6.3 Search Analytics
- Track popular search terms
- Monitor search performance
- A/B testing for search relevance

## Implementation Steps

### Step 1: Enhance SearchItem DTO
```csharp
// src/Application/DTOs/Search/SearchItemIndexDto.cs
// Enhanced version with additional fields for better search
```

### Step 2: Create SearchQuery DTO
```csharp
// src/Application/DTOs/Search/SearchQuery.cs
// Comprehensive search query parameters
```

### Step 3: Create SearchItemService
```csharp
// src/Application/Service/SearchItemService.cs
// Unified search service implementation
```

### Step 4: Update SearchController
```csharp
// src/Application/Controllers/SearchController.cs
// Add unified search endpoints
```

### Step 5: Create Mapping Profiles
```csharp
// Update existing profiles or create new ones for SearchItem mapping
```

### Step 6: Integration with Existing Services
- Update all domain services to trigger search indexing
- Implement background workers for bulk indexing

### Step 7: Testing and Validation
- Unit tests for search functionality
- Integration tests for end-to-end search
- Performance testing with large datasets

## Expected Benefits

1. **Unified Search Experience**: Single endpoint for searching across all entities
2. **Better Performance**: Optimized Elasticsearch queries with proper indexing
3. **Enhanced User Experience**: Auto-complete, suggestions, and faceted search
4. **Geographic Search**: Location-based search capabilities
5. **Vietnamese Language Support**: Proper text analysis for Vietnamese content
6. **Scalability**: Efficient indexing and search strategies for large datasets
7. **Analytics**: Search insights and performance monitoring

## Migration Strategy

1. **Backward Compatibility**: Keep existing museum search endpoints
2. **Gradual Migration**: Implement unified search alongside existing functionality
3. **Feature Toggle**: Allow switching between old and new search implementations
4. **Data Migration**: Bulk index existing data into the new unified index
5. **Monitoring**: Track performance and accuracy during transition 