using AutoMapper;
using Domain.Payment;

public class OrderEventDto
{
    public Guid OrderId { get; set; }
    public Guid EventId { get; set; }
    public float UnitPrice { get; set; }
    public EventDto? Event { get; set; }
}
public class OrderEventProfile : Profile
{
    public OrderEventProfile()
    {
        CreateMap<OrderEvent, OrderEventDto>().ReverseMap();
    }
}