namespace Application.Service;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Domain.Museums;
using Infrastructure.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Application.Shared.Type;
using Database;
using Application.Shared.Enum;
using Application.DTOs.MuseumRequest;
using Application.DTOs.Pagination;
using Application.DTOs.MuseumPolicy;
using Application.Shared.Constant;
using Domain.Users;

public interface IMuseumService
{
  Task<IActionResult> HandleGetAll(MuseumQuery query);
  Task<IActionResult> HandleGetAllAdmin(MuseumQuery query);
  Task<IActionResult> HandleGetById(Guid id);
  Task<IActionResult> HandleCreate(MuseumCreateDto dto);
  Task<IActionResult> HandleUpdate(Guid id, MuseumUpdateDto dto);
  Task<IActionResult> HandleDelete(Guid id);

  // MuseumRequest endpoints
  Task<IActionResult> HandleGetAllRequests(MuseumRequestQuery query);
  Task<IActionResult> HandleGetRequestById(Guid id);
  Task<IActionResult> HandleCreateRequest(MuseumRequestCreateDto dto);
  Task<IActionResult> HandleUpdateRequest(Guid id, MuseumRequestUpdateDto dto);
  Task<IActionResult> HandleDeleteRequest(Guid id);
  Task<IActionResult> HandleApproveRequest(Guid id);
  Task<IActionResult> HandleRejectRequest(Guid id);

  // MuseumPolicy methods
  Task<IActionResult> HandleGetAllPolicies(PaginationReq query, Guid museumId);
  Task<IActionResult> HandleGetPolicyById(Guid id);
  Task<IActionResult> HandleCreatePolicy(MuseumPolicyCreateDto dto);
  Task<IActionResult> HandleUpdatePolicy(Guid id, MuseumPolicyUpdateDto dto);
  Task<IActionResult> HandleDeletePolicy(Guid id);
}

public class MuseumService : BaseService, IMuseumService
{
  private readonly IMuseumRepository _museumRepository;
  private readonly IMuseumRequestRepository _museumRequestRepository;
  private readonly IMuseumPolicyRepository _museumPolicyRepository;
  private readonly IUserRoleRepository _userRoleRepository;
  private readonly IRoleRepository _roleRepository;

  public MuseumService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpCtx)
    : base(dbContext, mapper, httpCtx)
  {
    _museumRepository = new MuseumRepository(dbContext);
    _museumRequestRepository = new MuseumRequestRepository(dbContext);
    _museumPolicyRepository = new MuseumPolicyRepository(dbContext);
    _userRoleRepository = new UserRoleRepository(dbContext);
    _roleRepository = new RoleRepository(dbContext);
  }

  public async Task<IActionResult> HandleGetAll(MuseumQuery query)
  {
    var museums = _museumRepository.GetAll(query);
    var museumDtos = _mapper.Map<IEnumerable<MuseumDto>>(museums.Museums);

    return SuccessResp.Ok(new
    {
      List = museumDtos,
      Total = museums.Total
    });
  }

  public async Task<IActionResult> HandleGetAllAdmin(MuseumQuery query)
  {
    var museums = _museumRepository.GetAll(query);
    var museumDtos = _mapper.Map<IEnumerable<MuseumDto>>(museums.Museums);

    return SuccessResp.Ok(new
    {
      List = museumDtos,
      Total = museums.Total
    });
  }

  public async Task<IActionResult> HandleGetById(Guid id)
  {
    var museum = _museumRepository.GetById(id);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }
    var museumDto = _mapper.Map<MuseumDto>(museum);
    return SuccessResp.Ok(museumDto);
  }

  public async Task<IActionResult> HandleCreate(MuseumCreateDto dto)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var museum = _mapper.Map<Museum>(dto);
    museum.CreatedBy = payload.UserId;
    museum.Status = MuseumStatusEnum.NotVerified;
    await _museumRepository.AddAsync(museum);

    var museumDto = _mapper.Map<MuseumDto>(museum);
    return SuccessResp.Created(museumDto);
  }

  public async Task<IActionResult> HandleUpdate(Guid id, MuseumUpdateDto dto)
  {
    var museum = _museumRepository.GetById(id);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }
    _mapper.Map(dto, museum);
    await _museumRepository.UpdateAsync(id, museum);

    var museumDto = _mapper.Map<MuseumDto>(museum);
    return SuccessResp.Ok(museumDto);
  }

  public async Task<IActionResult> HandleDelete(Guid id)
  {
    var museum = _museumRepository.GetById(id);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }
    await _museumRepository.DeleteAsync(museum);
    return SuccessResp.Ok("Museum deleted successfully");
  }

  public async Task<IActionResult> HandleGetAllRequests(MuseumRequestQuery query)
  {
    var requests = _museumRequestRepository.GetAll(query);
    var requestDtos = _mapper.Map<IEnumerable<MuseumRequestDto>>(requests.Requests);

    return SuccessResp.Ok(new
    {
      List = requestDtos,
      Total = requests.Total
    });
  }

  public async Task<IActionResult> HandleGetRequestById(Guid id)
  {
    var request = _museumRequestRepository.GetById(id);
    if (request == null)
    {
      return ErrorResp.NotFound("Museum request not found");
    }
    var requestDto = _mapper.Map<MuseumRequestDto>(request);
    return SuccessResp.Ok(requestDto);
  }

  public async Task<IActionResult> HandleCreateRequest(MuseumRequestCreateDto dto)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var museum = _museumRepository.GetByName(dto.MuseumName);
    if (museum != null)
    {
      return ErrorResp.BadRequest("Museum already exists");
    }

    var request = _mapper.Map<MuseumRequest>(dto);
    request.CreatedBy = payload.UserId;
    request.Status = RequestStatusEnum.Pending;
    request.SubmittedAt = DateTime.UtcNow;

    await _museumRequestRepository.AddAsync(request);

    var requestDto = _mapper.Map<MuseumRequestDto>(request);
    return SuccessResp.Created(requestDto);
  }

  public async Task<IActionResult> HandleUpdateRequest(Guid id, MuseumRequestUpdateDto dto)
  {
    var request = _museumRequestRepository.GetById(id);
    if (request == null)
    {
      return ErrorResp.NotFound("Museum request not found");
    }

    if (request.Status != RequestStatusEnum.Pending)
    {
      return ErrorResp.BadRequest("Cannot update a request that is not pending");
    }

    _mapper.Map(dto, request);
    await _museumRequestRepository.UpdateAsync(id, request);

    var requestDto = _mapper.Map<MuseumRequestDto>(request);
    return SuccessResp.Ok(requestDto);
  }

  public async Task<IActionResult> HandleDeleteRequest(Guid id)
  {
    var request = _museumRequestRepository.GetById(id);
    if (request == null)
    {
      return ErrorResp.NotFound("Museum request not found");
    }

    await _museumRequestRepository.DeleteAsync(request);
    return SuccessResp.Ok("Museum request deleted successfully");
  }

  public async Task<IActionResult> HandleApproveRequest(Guid id)
  {
    var request = _museumRequestRepository.GetById(id);
    if (request == null)
    {
      return ErrorResp.NotFound("Museum request not found");
    }

    if (request.Status != RequestStatusEnum.Pending)
    {
      return ErrorResp.BadRequest("Can only approve pending requests");
    }

    request.Status = RequestStatusEnum.Approved;
    await _museumRequestRepository.UpdateAsync(id, request);

    // Create museum from approved request
    var museum = new Museum
    {
      Name = request.MuseumName,
      Description = request.MuseumDescription,
      Location = request.Location,
      ContactEmail = request.ContactEmail,
      ContactPhone = request.ContactPhone,
      Status = MuseumStatusEnum.Active,
      CreatedBy = request.CreatedBy
    };

    museum = await _museumRepository.AddAsync(museum);

    // run in thread add museum manager role to the user
    // create new thread
    var role = _roleRepository.GetRoleByName(UserConst.ROLE_MUSEUM_MANAGER);
    if (role != null)
    {
      var userRole = new UserRole
      {
        UserId = request.CreatedBy,
        RoleId = role.Id,
        MuseumId = museum.Id.ToString()
      };
      await _userRoleRepository.AddAsync(userRole);
    }

    var requestDto = _mapper.Map<MuseumRequestDto>(request);
    return SuccessResp.Ok(requestDto);
  }

  public async Task<IActionResult> HandleRejectRequest(Guid id)
  {
    var request = _museumRequestRepository.GetById(id);
    if (request == null)
    {
      return ErrorResp.NotFound("Museum request not found");
    }

    if (request.Status != RequestStatusEnum.Pending)
    {
      return ErrorResp.BadRequest("Can only reject pending requests");
    }

    request.Status = RequestStatusEnum.Rejected;
    await _museumRequestRepository.UpdateAsync(id, request);

    var requestDto = _mapper.Map<MuseumRequestDto>(request);
    return SuccessResp.Ok(requestDto);
  }

  public async Task<IActionResult> HandleGetAllPolicies(PaginationReq query, Guid museumId)
  {
    var policies = _museumPolicyRepository.GetAll(query, museumId);
    var policyDtos = _mapper.Map<IEnumerable<MuseumPolicyDto>>(policies.Policies);

    return SuccessResp.Ok(new
    {
      List = policyDtos,
      Total = policies.Total
    });
  }

  public async Task<IActionResult> HandleGetPolicyById(Guid id)
  {
    var policy = _museumPolicyRepository.GetById(id);
    if (policy == null)
    {
      return ErrorResp.NotFound("Museum policy not found");
    }
    var policyDto = _mapper.Map<MuseumPolicyDto>(policy);
    return SuccessResp.Ok(policyDto);
  }

  public async Task<IActionResult> HandleCreatePolicy(MuseumPolicyCreateDto dto)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var museum = _museumRepository.GetById(dto.MuseumId);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }

    var policy = _mapper.Map<MuseumPolicy>(dto);
    policy.CreatedBy = payload.UserId;
    policy.IsActive = true;

    await _museumPolicyRepository.AddAsync(policy);

    var policyDto = _mapper.Map<MuseumPolicyDto>(policy);
    return SuccessResp.Created(policyDto);
  }

  public async Task<IActionResult> HandleUpdatePolicy(Guid id, MuseumPolicyUpdateDto dto)
  {
    var policy = _museumPolicyRepository.GetById(id);
    if (policy == null)
    {
      return ErrorResp.NotFound("Museum policy not found");
    }

    _mapper.Map(dto, policy);
    await _museumPolicyRepository.UpdateAsync(id, policy);

    var policyDto = _mapper.Map<MuseumPolicyDto>(policy);
    return SuccessResp.Ok(policyDto);
  }

  public async Task<IActionResult> HandleDeletePolicy(Guid id)
  {
    var policy = _museumPolicyRepository.GetById(id);
    if (policy == null)
    {
      return ErrorResp.NotFound("Museum policy not found");
    }

    await _museumPolicyRepository.DeleteAsync(policy);
    return SuccessResp.Ok("Museum policy deleted successfully");
  }
}