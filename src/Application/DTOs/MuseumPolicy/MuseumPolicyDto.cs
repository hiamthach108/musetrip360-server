namespace Application.DTOs.MuseumPolicy;

using Application.Shared.Enum;
using Application.DTOs.User;
using AutoMapper;
using Domain.Museums;
using Application.DTOs.Museum;

public class MuseumPolicyDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = null!;
  public string Content { get; set; } = null!;
  public PolicyTypeEnum PolicyType { get; set; }
  public bool IsActive { get; set; }
  public Guid MuseumId { get; set; }
  public Guid CreatedBy { get; set; }
  public UserDto CreatedByUser { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public MuseumDto Museum { get; set; } = null!;
}

public class MuseumPolicyCreateDto
{
  public string Title { get; set; } = null!;
  public string Content { get; set; } = null!;
  public PolicyTypeEnum PolicyType { get; set; }
  public Guid MuseumId { get; set; }
}

public class MuseumPolicyUpdateDto
{
  public string? Title { get; set; }
  public string? Content { get; set; }
  public PolicyTypeEnum? PolicyType { get; set; }
  public bool? IsActive { get; set; }
}

public class MuseumPolicyProfile : Profile
{
  public MuseumPolicyProfile()
  {
    CreateMap<MuseumPolicy, MuseumPolicyDto>().ReverseMap();

    CreateMap<MuseumPolicyCreateDto, MuseumPolicy>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<MuseumPolicyUpdateDto, MuseumPolicy>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}