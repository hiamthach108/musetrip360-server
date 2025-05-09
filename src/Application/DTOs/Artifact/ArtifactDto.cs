using System.Collections.Generic;
using AutoMapper;
using Domain.Artifacts;
using System.Text.Json;
public class ArtifactDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string HistoricalPeriod { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string Model3DUrl { get; set; } = null!;
    public float Rating { get; set; }
    public bool IsActive { get; set; }
    public Guid MuseumId { get; set; }
    public Guid CreatedBy { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<EventDto> Events { get; set; } = null!;
}
public class ArtifactProfile : Profile
{
    public ArtifactProfile()
    {
        CreateMap<Artifact, ArtifactDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ArtifactCreateDto, Artifact>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
        .ForMember(dest => dest.MuseumId, opt => opt.Ignore())
        .ForMember(dest => dest.Rating, opt => opt.Ignore())
        .ForMember(dest => dest.IsActive, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.Events, opt => opt.Ignore())
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ArtifactUpdateDto, Artifact>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
        .ForMember(dest => dest.MuseumId, opt => opt.Ignore())
        .ForMember(dest => dest.Rating, opt => opt.Ignore())
        .ForMember(dest => dest.IsActive, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.Events, opt => opt.Ignore())
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
