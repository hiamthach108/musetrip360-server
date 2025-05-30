namespace Application.DTOs.MuseumRequest;

using Application.Shared.Enum;
using Application.DTOs.User;
using Application.DTOs.Pagination;
using System.Text.Json;
using AutoMapper;
using Domain.Museums;

public class MuseumRequestDto
{
  public Guid Id { get; set; }
  public string MuseumName { get; set; } = null!;
  public string MuseumDescription { get; set; } = null!;
  public string Location { get; set; } = null!;
  public string ContactEmail { get; set; } = null!;
  public string ContactPhone { get; set; } = null!;
  public DateTime SubmittedAt { get; set; }
  public RequestStatusEnum Status { get; set; }
  public Guid CreatedBy { get; set; }
  public UserDto CreatedByUser { get; set; } = null!;

  public JsonDocument? Metadata { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class MuseumRequestCreateDto
{
  public string MuseumName { get; set; } = null!;
  public string MuseumDescription { get; set; } = null!;
  public string Location { get; set; } = null!;
  public string ContactEmail { get; set; } = null!;
  public string ContactPhone { get; set; } = null!;
  public JsonDocument? Metadata { get; set; }
}

public class MuseumRequestUpdateDto
{
  public string? MuseumName { get; set; }
  public string? MuseumDescription { get; set; }
  public string? Location { get; set; }
  public string? ContactEmail { get; set; }
  public string? ContactPhone { get; set; }
}

public class MuseumRequestQuery : PaginationReq
{
  public string? Search { get; set; }
}

public class MuseumRequestProfile : Profile
{
  public MuseumRequestProfile()
  {
    CreateMap<MuseumRequest, MuseumRequestDto>().ReverseMap();

    CreateMap<MuseumRequestCreateDto, MuseumRequest>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<MuseumRequestUpdateDto, MuseumRequest>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}