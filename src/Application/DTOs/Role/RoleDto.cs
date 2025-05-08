namespace Application.DTOs.Role;

using AutoMapper;
using Domain.Rolebase;

public class RoleDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  public bool IsActive { get; set; }
  public ICollection<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
}

public class RoleCreateDto
{
  public string Name { get; set; } = null!;
  public string Description { get; set; } = null!;
  // public ICollection<Guid> PermissionIds { get; set; } = new List<Guid>();
}

public class RoleUpdateDto
{
  public string? Name { get; set; }
  public string? Description { get; set; }
  public bool? IsActive { get; set; }
  // public ICollection<Guid>? PermissionIds { get; set; }
}

public class RoleQuery
{
  public string? Search { get; set; }
  public int Page { get; set; } = 0;
  public int PageSize { get; set; } = 10;
}

public class RoleProfile : Profile
{
  public RoleProfile()
  {
    CreateMap<Role, RoleDto>();
    CreateMap<RoleCreateDto, Role>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<Role, RoleCreateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<RoleUpdateDto, Role>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<Role, RoleUpdateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    CreateMap<Permission, PermissionDto>();
  }
}