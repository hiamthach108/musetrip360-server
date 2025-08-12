namespace Application.DTOs.Payment;

using System.Text.Json;
using Application.Shared.Enum;
using AutoMapper;

public class CreateOrderReq
{
  public float TotalAmount { get; set; }

  public OrderTypeEnum OrderType { get; set; }

  public JsonDocument? Metadata { get; set; }
  public List<Guid> ItemIds { get; set; } = [];
}

public class CreateOrderMsg
{
  public float TotalAmount { get; set; }

  public OrderTypeEnum OrderType { get; set; }

  public JsonDocument? Metadata { get; set; }
  public List<Guid> ItemIds { get; set; } = [];

  public Guid CreatedBy { get; set; }
}

public class CreateOrderProfile : Profile
{
  public CreateOrderProfile()
  {
    CreateMap<CreateOrderReq, CreateOrderMsg>()
      .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
  }
}