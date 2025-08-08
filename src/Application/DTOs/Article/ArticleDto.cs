namespace Application.DTOs.Article;

using System.Text.Json;
using AutoMapper;
using Domain.Museums;
using Application.Shared.Enum;
using Application.DTOs.User;
using Application.DTOs.Museum;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

public class ArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public ArticleStatusEnum Status { get; set; }
    public DateTime PublishedAt { get; set; }
    public Guid MuseumId { get; set; }
    public Guid CreatedBy { get; set; }

    public DataEntityType DataEntityType { get; set; }
    public Guid EntityId { get; set; }
    public JsonDocument? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public MuseumDto Museum { get; set; } = null!;
    public UserDto CreatedByUser { get; set; } = null!;
}

public class ArticleProfile : Profile
{
    public ArticleProfile()
    {
        CreateMap<Article, ArticleDto>().ReverseMap();
        CreateMap<ArticleCreateDto, Article>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ArticleUpdateDto, Article>().ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}