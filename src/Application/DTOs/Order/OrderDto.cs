namespace Application.DTOs.Order;

using Domain.Payment;
using Application.Shared.Enum;
using AutoMapper;
using Application.DTOs.User;
using System.Text.Json;

public class OrderDto
{
  public Guid Id { get; set; }
  public float TotalAmount { get; set; }
  public PaymentStatusEnum Status { get; set; }
  public OrderTypeEnum OrderType { get; set; }
  public string OrderCode { get; set; } = null!;
  public DateTime ExpiredAt { get; set; }
  public JsonDocument? Metadata { get; set; }
  public Guid CreatedBy { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public UserDto CreatedByUser { get; set; } = null!;
  public ICollection<OrderEventDto> OrderEvents { get; set; } = [];
  public ICollection<OrderTourDto> OrderTours { get; set; } = [];
}

public class OrderProfile : Profile
{
  public OrderProfile()
  {
    CreateMap<Order, OrderDto>().ReverseMap();
  }
}