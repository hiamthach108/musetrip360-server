using Application.Shared.Enum;
using AutoMapper;
using Domain.Events;

public class EventParticipantDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public ParticipantRoleEnum Role { get; set; }
    public ParticipantStatusEnum Status { get; set; }
}
public class EventParticipantProfile : Profile
{
    public EventParticipantProfile()
    {
        CreateMap<EventParticipant, EventParticipantDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<EventParticipantCreateDto, EventParticipant>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<EventParticipantUpdateDto, EventParticipant>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}