namespace Application.DTOs.Plan;

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Domain.Subscription;

public class PlanDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public decimal Price { get; set; }
  public int DurationDays { get; set; }
  public int? MaxEvents { get; set; }
  public decimal? DiscountPercent { get; set; }
  public bool IsActive { get; set; }
  public int SubscriptionCount { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class PlanCreateDto
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = string.Empty;

  [StringLength(1000)]
  public string? Description { get; set; }

  [Required]
  [Range(0.01, double.MaxValue)]
  public decimal Price { get; set; }

  [Required]
  [Range(1, int.MaxValue)]
  public int DurationDays { get; set; }

  [Range(1, int.MaxValue)]
  public int? MaxEvents { get; set; }

  [Range(0, 100)]
  public decimal? DiscountPercent { get; set; }

  public bool IsActive { get; set; } = true;
}

public class PlanUpdateDto
{
  [StringLength(100)]
  public string? Name { get; set; }

  [StringLength(1000)]
  public string? Description { get; set; }

  [Range(0.01, double.MaxValue)]
  public decimal? Price { get; set; }

  [Range(1, int.MaxValue)]
  public int? DurationDays { get; set; }

  [Range(1, int.MaxValue)]
  public int? MaxEvents { get; set; }

  [Range(0, 100)]
  public decimal? DiscountPercent { get; set; }

  public bool? IsActive { get; set; }
}

public class PlanQuery
{
  public string? Name { get; set; }
  public decimal? MinPrice { get; set; }
  public decimal? MaxPrice { get; set; }
  public int? MinDurationDays { get; set; }
  public int? MaxDurationDays { get; set; }
  public bool? IsActive { get; set; }
  public int Page { get; set; } = 1;
  public int PageSize { get; set; } = 10;
  public string? SortBy { get; set; } = "Name";
  public string? SortOrder { get; set; } = "asc";
}

public class PlanSummaryDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public int DurationDays { get; set; }
  public bool IsActive { get; set; }
  public int SubscriptionCount { get; set; }
}

public class PlanProfile : Profile
{
  public PlanProfile()
  {
    CreateMap<Plan, PlanDto>()
        .ForMember(dest => dest.SubscriptionCount, opt => opt.MapFrom(src => src.Subscriptions.Count));

    CreateMap<Plan, PlanSummaryDto>()
        .ForMember(dest => dest.SubscriptionCount, opt => opt.MapFrom(src => src.Subscriptions.Count));

    CreateMap<PlanCreateDto, Plan>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    CreateMap<PlanUpdateDto, Plan>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}