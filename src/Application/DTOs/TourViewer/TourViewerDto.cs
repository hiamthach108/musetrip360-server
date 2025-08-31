using AutoMapper;
using Domain.Tours;
using Domain.Users;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

public class TourViewerDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TourId { get; set; }
    public string AccessType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? LastViewedAt { get; set; }

    public User User { get; set; } = null!;
    public TourOnline TourOnline { get; set; } = null!;
}
public class TourViewerProfile : Profile
{
    public TourViewerProfile()
    {
        CreateMap<TourViewer, TourViewerDto>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}