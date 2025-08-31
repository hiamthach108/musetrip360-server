using AutoMapper;
using Domain.Payment;

public class OrderTourDto
{
    public Guid OrderId { get; set; }
    public Guid TourId { get; set; }
    public float UnitPrice { get; set; }
    public TourOnlineDto? TourOnline { get; set; }
}
public class OrderTourProfile : Profile
{
    public OrderTourProfile()
    {
        CreateMap<OrderTour, OrderTourDto>().ReverseMap();
    }
}