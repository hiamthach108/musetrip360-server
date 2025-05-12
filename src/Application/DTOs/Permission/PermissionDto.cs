namespace Application.DTOs.Role;

using AutoMapper;
using Domain.Rolebase;

public class PermissionDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string? ResourceGroup { get; set; }
  public bool IsActive { get; set; }
}

public class PermissionCreateDto
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string? ResourceGroup { get; set; }
}

public class PermissionUpdateDto
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public string? ResourceGroup { get; set; }
  public bool IsActive { get; set; }
}

public class PermissionQuery
{
  public string? Search { get; set; }
  public int Page { get; set; } = 0;
  public int PageSize { get; set; } = 10;
}

public class PermissionProfile : Profile
{
  public PermissionProfile()
  {
    CreateMap<Permission, PermissionDto>();
    CreateMap<PermissionCreateDto, Permission>();
    CreateMap<PermissionUpdateDto, Permission>();
  }
}