namespace Application.DTOs.Feedback;

using AutoMapper;
using Domain.Reviews;
using Application.DTOs.User;
using Application.Shared.Enum;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid TargetId { get; set; }
    public DataEntityType Type { get; set; }

    public string Comment { get; set; } = null!;
    public int Rating { get; set; }
    public UserDto CreatedByUser { get; set; } = null!;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
public class FeedbackProfile : Profile
{
    public FeedbackProfile()
    {
        CreateMap<Feedback, FeedbackDto>()
        .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}