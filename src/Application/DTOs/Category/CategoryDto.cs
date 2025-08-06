namespace Application.DTOs.Category;

using AutoMapper;
using Domain.Content;
using System.Text.Json;

public class CategoryDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public JsonDocument? Metadata { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
}

public class CategoryCreateDto
{
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public JsonDocument? Metadata { get; set; }
}

public class CategoryUpdateDto
{
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public JsonDocument? Metadata { get; set; }
}

public class CategoryProfile : Profile
{
  public CategoryProfile()
  {
    CreateMap<Category, CategoryDto>();
    CreateMap<CategoryCreateDto, Category>()
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<CategoryUpdateDto, Category>()
      .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}