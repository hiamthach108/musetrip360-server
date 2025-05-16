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
}
public class ArtifactProfile : Profile
{
    public ArtifactProfile()
    {
        CreateMap<Artifact, ArtifactDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ArtifactCreateDto, Artifact>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ArtifactUpdateDto, Artifact>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
