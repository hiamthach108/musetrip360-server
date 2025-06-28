namespace Application.DTOs.Order;

using Domain.Payment;
using Application.DTOs.User;
using Application.Shared.Enum;
using AutoMapper;

public class OrderDto
{
  public Guid Id { get; set; }
  public float TotalAmount { get; set; }
  public PaymentStatusEnum Status { get; set; }
  public OrderTypeEnum OrderType { get; set; }
  public string? Metadata { get; set; }
  public UserDto CreatedBy { get; set; } = null!;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class OrderProfile : Profile
{
  public OrderProfile()
  {
    CreateMap<Order, OrderDto>().ReverseMap();
  }
}