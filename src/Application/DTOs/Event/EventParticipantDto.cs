using Application.DTOs.User;
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
    public UserDto User { get; set; } = null!;
    public EventDto Event { get; set; } = null!;
<<<<<<< HEAD
}
public class EventParticipantNoNavigateDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }
    public ParticipantRoleEnum Role { get; set; }
    public ParticipantStatusEnum Status { get; set; }
=======
>>>>>>> 83b839101e229ac246323aebbd1cc254e07c6059
}
public class EventParticipantProfile : Profile
{
    public EventParticipantProfile()
    {
        CreateMap<EventParticipant, EventParticipantDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<EventParticipant, EventParticipantNoNavigateDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<EventParticipantCreateDto, EventParticipant>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<EventParticipantUpdateDto, EventParticipant>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}