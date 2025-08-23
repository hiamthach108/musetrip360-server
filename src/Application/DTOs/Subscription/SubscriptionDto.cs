namespace Application.DTOs.Subscription;

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.DTOs.Museum;
using Application.DTOs.Plan;
using Application.DTOs.User;
using Application.Shared.Enum;
using AutoMapper;
using Domain.Subscription;

public class SubscriptionDto
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public Guid PlanId { get; set; }
  public Guid OrderId { get; set; }
  public Guid MuseumId { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public SubscriptionStatusEnum Status { get; set; }
  public JsonDocument? Metadata { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public UserDto? User { get; set; }
  public PlanDto? Plan { get; set; }
  public MuseumDto? Museum { get; set; }
}

public class SubscriptionCreateDto
{
  [Required]
  public Guid PlanId { get; set; }

  [Required]
  public Guid MuseumId { get; set; }
}

public class BuySubscriptionDto
{
  [Required]
  public Guid PlanId { get; set; }

  [Required]
  public Guid MuseumId { get; set; }

  public string? SuccessUrl { get; set; }

  public string? CancelUrl { get; set; }
  public JsonDocument? Metadata { get; set; }
}

public class SubscriptionQuery
{
  public Guid? MuseumId { get; set; }
  public Guid? PlanId { get; set; }
  public SubscriptionStatusEnum? Status { get; set; }
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 10;
}

public class SubscriptionSummaryDto
{
  public Guid Id { get; set; }
  public Guid MuseumId { get; set; }
  public string MuseumName { get; set; } = string.Empty;
  public string PlanName { get; set; } = string.Empty;
  public decimal PlanPrice { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime EndDate { get; set; }
  public SubscriptionStatusEnum Status { get; set; }
  public int DaysRemaining { get; set; }
}

public class SubscriptionProfile : Profile
{
  public SubscriptionProfile()
  {
    CreateMap<Subscription, SubscriptionDto>();

    CreateMap<Subscription, SubscriptionSummaryDto>()
        .ForMember(dest => dest.MuseumName, opt => opt.MapFrom(src => src.Museum.Name))
        .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plan.Name))
        .ForMember(dest => dest.PlanPrice, opt => opt.MapFrom(src => src.Plan.Price))
        .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src =>
            src.Status == SubscriptionStatusEnum.Active
                ? Math.Max(0, (src.EndDate - DateTime.UtcNow).Days)
                : 0));

    CreateMap<SubscriptionCreateDto, Subscription>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}