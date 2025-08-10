namespace Application.DTOs.Search;

using AutoMapper;
using Domain.Museums;
using Domain.Artifacts;
using Domain.Events;
using Domain.Tours;

public class SemanticSearchQuery
{
  public string Query { get; set; } = "";
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 10;
  public string? Type { get; set; }
  public decimal? MinSimilarity { get; set; } = 0.7m;
  public bool IncludeEmbeddings { get; set; } = false;
}

public class SemanticSearchItemDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = "";
  public string Type { get; set; } = "";
  public string? Description { get; set; }
  public string SearchText { get; set; } = "";
  public string[] Tags { get; set; } = Array.Empty<string>();
  public float[]? Embedding { get; set; }
  public float SimilarityScore { get; set; }
}

public class SemanticSearchResultDto
{
  public List<SemanticSearchItemDto> Items { get; set; } = new();
  public int Total { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
  public string Query { get; set; } = "";
  public float MaxSimilarityScore { get; set; }
  public float MinSimilarityScore { get; set; }
  public Dictionary<string, int> TypeAggregations { get; set; } = new();
}

public class EmbeddingRequestDto
{
  public string Text { get; set; } = "";
  public string? Type { get; set; }
  public Guid? EntityId { get; set; }
}

public class EmbeddingResponseDto
{
  public float[] Embedding { get; set; } = Array.Empty<float>();
  public int Dimensions { get; set; }
  public string ProcessedAt { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
}

public class SemanticSearchProfile : Profile
{
  public SemanticSearchProfile()
  {
    CreateMap<SearchItemIndexDto, SemanticSearchItemDto>()
      .ForMember(dest => dest.SimilarityScore, opt => opt.Ignore())
      .ForMember(dest => dest.Embedding, opt => opt.Ignore());


    CreateMap<Museum, SemanticSearchItemDto>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Museum"))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description} {src.Location}"))
      .ForMember(dest => dest.SimilarityScore, opt => opt.Ignore())
      .ForMember(dest => dest.Embedding, opt => opt.Ignore());

    CreateMap<Artifact, SemanticSearchItemDto>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Artifact"))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description}"))
      .ForMember(dest => dest.SimilarityScore, opt => opt.Ignore())
      .ForMember(dest => dest.Embedding, opt => opt.Ignore());

    CreateMap<Event, SemanticSearchItemDto>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Event"))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Title} {src.Description} {src.Location}"))
      .ForMember(dest => dest.SimilarityScore, opt => opt.Ignore())
      .ForMember(dest => dest.Embedding, opt => opt.Ignore());

    CreateMap<TourContent, SemanticSearchItemDto>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => $"Tour Content #{src.ZOrder}"))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "TourContent"))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.Content))
      .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Content.Length > 200 ? src.Content.Substring(0, 200) + "..." : src.Content))
      .ForMember(dest => dest.SimilarityScore, opt => opt.Ignore())
      .ForMember(dest => dest.Embedding, opt => opt.Ignore());

    CreateMap<TourOnline, SemanticSearchItemDto>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "TourOnline"))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description}"))
      .ForMember(dest => dest.SimilarityScore, opt => opt.Ignore())
      .ForMember(dest => dest.Embedding, opt => opt.Ignore());
  }
}