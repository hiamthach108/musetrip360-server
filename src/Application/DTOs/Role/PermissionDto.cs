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

public class PermissionProfile : Profile
{
  public PermissionProfile()
  {
    CreateMap<Permission, PermissionDto>();
  }
}