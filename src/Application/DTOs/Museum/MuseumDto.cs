namespace Application.DTOs.Museum;

using System;
using AutoMapper;
using Domain.Museums;
using System.Text.Json.Serialization;
using Application.Shared.Enum;
using System.Text.Json;
using Application.DTOs.Category;

public class MuseumDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string Location { get; set; } = null!;
  public string ContactEmail { get; set; } = null!;
  public string ContactPhone { get; set; } = null!;
  public double Rating { get; set; }
  public decimal Latitude { get; set; }
  public decimal Longitude { get; set; }
  public Guid CreatedBy { get; set; }

  [JsonConverter(typeof(JsonStringEnumConverter))]
  public MuseumStatusEnum Status { get; set; }
  public JsonDocument? Metadata { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}

public class MuseumProfile : Profile
{
  public MuseumProfile()
  {
    CreateMap<Museum, MuseumDto>()
      .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories));
    CreateMap<MuseumDto, Museum>()
      .ForMember(dest => dest.Categories, opt => opt.Ignore());

    // Elasticsearch mapping
    CreateMap<Museum, MuseumIndexDto>();
    CreateMap<MuseumIndexDto, MuseumDto>();

    // ignore null
    CreateMap<Museum, MuseumCreateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<MuseumCreateDto, Museum>()
      .ForMember(dest => dest.Categories, opt => opt.Ignore())
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    CreateMap<Museum, MuseumUpdateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<MuseumUpdateDto, Museum>()
      .ForMember(dest => dest.Status, opt => opt.PreCondition(src => src.Status.HasValue))
      .ForMember(dest => dest.Categories, opt => opt.Ignore())
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}