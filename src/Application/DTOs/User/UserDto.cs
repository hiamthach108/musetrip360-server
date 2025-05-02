namespace Application.DTOs.User;

using Application.Shared.Enum;
using AutoMapper;
using Domain.Users;


public class UserDto
{
  public Guid Id { get; set; }
  public string Username { get; set; } = null!;
  public string FullName { get; set; } = null!;
  public string Email { get; set; } = null!;
  public string? PhoneNumber { get; set; }
  public string? AvatarUrl { get; set; }
  public DateTime? BirthDate { get; set; }
  public AuthTypeEnum AuthType { get; set; }
  public UserStatusEnum Status { get; set; }
  public DateTime LastLogin { get; set; }
}

public class UserBlobDto
{
  public Guid Id { get; set; }
  public string FullName { get; set; } = null!;
  public string? AvatarUrl { get; set; }
  public UserStatusEnum Status { get; set; }
}

public class UserProfile : Profile
{
  public UserProfile()
  {
    CreateMap<User, UserDto>();
    CreateMap<UserDto, User>();

    // ignore null
    CreateMap<User, UserCreateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<UserCreateDto, User>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    CreateMap<User, UserUpdateDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<UserUpdateDto, User>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

    CreateMap<User, UserBlobDto>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<User, UpdateProfileReq>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    CreateMap<UpdateProfileReq, User>().ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
  }
}