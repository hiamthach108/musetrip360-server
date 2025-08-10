namespace Application.DTOs.Search;

using AutoMapper;
using Domain.Museums;
using Domain.Artifacts;
using Domain.Events;
using Domain.Tours;

public class SearchItemIndexDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = "";
  public string Type { get; set; } = "";
  public string? Thumbnail { get; set; }
  public string? Description { get; set; }
  public decimal? Latitude { get; set; }
  public decimal? Longitude { get; set; }

  public string? Location { get; set; }
  public double? Rating { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public string Status { get; set; } = "";
  public Guid? RelatedEntityId { get; set; }

  public string SearchText { get; set; } = "";
  public string[] Tags { get; set; } = Array.Empty<string>();
}
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

public class SearchItemProfile : Profile
{
  public SearchItemProfile()
  {
    CreateMap<Museum, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Museum"))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description} {src.Location}"))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
      .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => (Guid?)null));

    CreateMap<Artifact, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Artifact"))
      .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.MuseumId))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description}"))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"))
      .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => (double)src.Rating))
      .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.ImageUrl));

    CreateMap<Event, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "Event"))
      .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.MuseumId))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Title} {src.Description} {src.Location}"))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
      .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location));

    CreateMap<TourContent, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => $"Tour Content #{src.ZOrder}"))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "TourContent"))
      .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.TourId))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => src.Content))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"))
      .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Content.Length > 200 ? src.Content.Substring(0, 200) + "..." : src.Content));

    CreateMap<TourOnline, SearchItemIndexDto>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "TourOnline"))
      .ForMember(dest => dest.RelatedEntityId, opt => opt.MapFrom(src => src.MuseumId))
      .ForMember(dest => dest.SearchText, opt => opt.MapFrom(src => $"{src.Name} {src.Description}"))
      .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"));

    CreateMap<SearchItemIndexDto, SearchItem>()
      .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
      .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
      .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
      .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
      .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))
      .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude));
  }
}