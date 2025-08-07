namespace Application.DTOs.HistoricalPeriod;

using AutoMapper;
using Domain.Content;
using System.Text.Json;

public class HistoricalPeriodDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public string? StartDate { get; set; }
  public string? EndDate { get; set; }
  public JsonDocument? Metadata { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class HistoricalPeriodCreateDto
{
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public string? StartDate { get; set; }
  public string? EndDate { get; set; }
  public JsonDocument? Metadata { get; set; }
}

public class HistoricalPeriodUpdateDto
{
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public string? StartDate { get; set; }
  public string? EndDate { get; set; }
  public JsonDocument? Metadata { get; set; }
}

public class HistoricalPeriodQueryDto
{
  public string? Name { get; set; }
}

public class HistoricalPeriodProfile : Profile
{
  public HistoricalPeriodProfile()
  {
    CreateMap<HistoricalPeriod, HistoricalPeriodDto>();
    CreateMap<HistoricalPeriodCreateDto, HistoricalPeriod>()
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<HistoricalPeriodUpdateDto, HistoricalPeriod>()
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}