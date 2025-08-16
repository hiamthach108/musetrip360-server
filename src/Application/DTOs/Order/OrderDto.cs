namespace Application.DTOs.Order;

using Domain.Payment;
using Application.Shared.Enum;
using AutoMapper;
using Application.DTOs.User;

public class OrderDto
{
  public Guid Id { get; set; }
  public float TotalAmount { get; set; }
  public PaymentStatusEnum Status { get; set; }
  public OrderTypeEnum OrderType { get; set; }
  public string? Metadata { get; set; }
  public Guid CreatedBy { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public UserDto CreatedByUser { get; set; } = null!;
}

public class OrderProfile : Profile
{
  public OrderProfile()
  {
    CreateMap<Order, OrderDto>().ReverseMap();
  }
}