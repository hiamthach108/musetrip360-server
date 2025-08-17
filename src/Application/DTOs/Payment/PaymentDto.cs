namespace Application.DTOs.Payment;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Shared.Enum;
using AutoMapper;

public class CreateOrderReq
{

  public OrderTypeEnum OrderType { get; set; }

  public JsonDocument? Metadata { get; set; }
  public List<Guid> ItemIds { get; set; } = [];
  [Required]
  public string CancelUrl { get; set; } = null!;
  [Required]
  public string ReturnUrl { get; set; } = null!;
}

public class CreateOrderMsg
{

  public OrderTypeEnum OrderType { get; set; }

  public JsonDocument? Metadata { get; set; }
  public List<Guid> ItemIds { get; set; } = [];

  public Guid CreatedBy { get; set; }
  public string OrderCode { get; set; } = null!;
  public float TotalAmount { get; set; }
  public DateTime ExpiredAt { get; set; }
}

public class CreateOrderProfile : Profile
{
  public CreateOrderProfile()
  {
    CreateMap<CreateOrderReq, CreateOrderMsg>()
      .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
  }
}