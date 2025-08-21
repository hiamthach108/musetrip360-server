namespace MuseTrip360.src.Application.DTOs.Feedback;
using AutoMapper;
using Domain.Reviews;
using Domain.Users;

public class FeedbackDto
{
    public string Comment { get; set; } = null!;
    public int Rating { get; set; }
    public User CreatedByUser { get; set; } = null!;
}
public class FeedbackProfile : Profile
{
    public FeedbackProfile()
    {
        CreateMap<Feedback, FeedbackDto>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}