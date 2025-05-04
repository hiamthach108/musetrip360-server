namespace Application.DTOs.Museums;

using System;
using AutoMapper;
using Domain.Museums;


public class MuseumDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string Location { get; set; } = null!;
  public string ContactEmail { get; set; } = null!;
  public string ContactPhone { get; set; } = null!;
  public double Rating { get; set; }
  public Guid CreatedBy { get; set; }
  public string Status { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class MuseumProfile : Profile
{
  public MuseumProfile()
  {
    CreateMap<Museum, MuseumDto>();
    CreateMap<MuseumDto, Museum>();

    // ignore null
    CreateMap<Museum, MuseumCreateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<MuseumCreateDto, Museum>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    CreateMap<Museum, MuseumUpdateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<MuseumUpdateDto, Museum>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}