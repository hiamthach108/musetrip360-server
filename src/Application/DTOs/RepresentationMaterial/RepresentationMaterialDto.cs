namespace Application.DTOs.RepresentationMaterial;

using System.Text.Json;
using AutoMapper;
using Domain.Content;

public class RepresentationMaterialDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int ZOrder { get; set; } = 0;
    public Guid CreatedBy { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class RepresentationMaterialProfile : Profile
{
    public RepresentationMaterialProfile()
    {
        CreateMap<RepresentationMaterial, RepresentationMaterialDto>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<RepresentationMaterialCreateDto, RepresentationMaterial>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<RepresentationMaterialUpdateDto, RepresentationMaterial>()
        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}