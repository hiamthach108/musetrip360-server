namespace Application.DTOs.UserRole;

using Application.DTOs.Role;
using Application.DTOs.User;
using Domain.Users;
using AutoMapper;


public class UserRoleDto
{
  public Guid UserId { get; set; }
  public Guid RoleId { get; set; }
  public string? MuseumId { get; set; }

  public UserDto User { get; set; } = null!;
  public RoleDto Role { get; set; } = null!;
}

public class UserRoleFormDto
{
  public Guid UserId { get; set; }
  public Guid RoleId { get; set; }
  public string? MuseumId { get; set; }
}

public class UserRoleProfile : Profile
{
  public UserRoleProfile()
  {
    CreateMap<UserRole, UserRoleDto>();
    CreateMap<UserRoleDto, UserRole>();

    CreateMap<UserRole, UserRoleFormDto>();
  }
}