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
  Task<IActionResult> HandleGetUserMuseums();
  Task<IActionResult> HandleGetById(Guid id);
  Task<IActionResult> HandleCreate(MuseumCreateDto dto);
  Task<IActionResult> HandleUpdate(Guid id, MuseumUpdateDto dto);
  Task<IActionResult> HandleDelete(Guid id);

  // MuseumRequest endpoints
  Task<IActionResult> HandleGetAllRequests(MuseumRequestQuery query);
  Task<IActionResult> HandleGetAllRequestsByUserId(MuseumRequestQuery query);
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
  Task<IActionResult> HandleBulkCreateUpdatePolicies(MuseumPolicyBulkRequestDto dto);
}

public class MuseumService : BaseService, IMuseumService
{
  private readonly IMuseumRepository _museumRepository;
  private readonly IMuseumRequestRepository _museumRequestRepository;
  private readonly IMuseumPolicyRepository _museumPolicyRepository;
  private readonly IUserRoleRepository _userRoleRepository;
  private readonly IRoleRepository _roleRepository;
  private readonly IUserService _userSvc;
  private readonly ISearchItemService _searchItemService;
  public MuseumService(
    MuseTrip360DbContext dbContext,
    IMapper mapper,
    IHttpContextAccessor httpCtx,
    IUserService userSvc,
    ISearchItemService searchItemService
  )
    : base(dbContext, mapper, httpCtx)
  {
    _museumRepository = new MuseumRepository(dbContext);
    _museumRequestRepository = new MuseumRequestRepository(dbContext);
    _museumPolicyRepository = new MuseumPolicyRepository(dbContext);
    _userRoleRepository = new UserRoleRepository(dbContext);
    _roleRepository = new RoleRepository(dbContext);
    _userSvc = userSvc;
    _searchItemService = searchItemService;
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
    var isAllowed = await _userSvc.ValidatePermission(PermissionConst.SYSTEM_MUSEUM, [PermissionConst.MUSEUMS_MANAGEMENT]);
    if (!isAllowed)
    {
      return ErrorResp.Forbidden("You are not allowed to access this resource");
    }

    var museums = _museumRepository.GetAllAdmin(query);
    var museumDtos = _mapper.Map<IEnumerable<MuseumDto>>(museums.Museums);

    return SuccessResp.Ok(new
    {
      List = museumDtos,
      Total = museums.Total
    });
  }

  public async Task<IActionResult> HandleGetUserMuseums()
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }
    var userRoles = _userRoleRepository.GetAllByUserId(payload.UserId);
    var museumIds = userRoles.Select(r => r.MuseumId).Where(id => Guid.TryParse(id, out _)).ToList();
    var museumIdsGuid = museumIds.Select(id => Guid.Parse(id)).ToList();
    var museums = _museumRepository.GetByIds(museumIdsGuid);
    var museumDtos = _mapper.Map<IEnumerable<MuseumDto>>(museums);
    return SuccessResp.Ok(museumDtos);
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

    // check if name is exists
    var isMuseumNameExists = _museumRepository.IsMuseumNameExists(dto.Name);
    if (isMuseumNameExists)
    {
      return ErrorResp.BadRequest("Museum name already exists");
    }

    var museum = _mapper.Map<Museum>(dto);
    museum.CreatedBy = payload.UserId;
    museum.Status = MuseumStatusEnum.NotVerified;
    await _museumRepository.AddAsync(museum);
    // Index in Elasticsearch service
    await _searchItemService.IndexMuseumAsync(museum.Id);

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

    // Update in Elasticsearch in separate thread
    if (museum.Status != MuseumStatusEnum.Active)
    {
      _ = Task.Run(async () =>
      {
        await _searchItemService.DeleteItemFromIndexAsync(id);
      });
    }
    else
    {
      _ = Task.Run(async () =>
      {
        await _searchItemService.IndexMuseumAsync(id);
      });
    }

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

    // Remove from Elasticsearch
    _ = Task.Run(async () =>
    {
      await _searchItemService.DeleteItemFromIndexAsync(id);
    });

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

  public async Task<IActionResult> HandleGetAllRequestsByUserId(MuseumRequestQuery query)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var requests = _museumRequestRepository.GetByUserId(payload.UserId, query);
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

    var isMuseumNameExists = _museumRepository.IsMuseumNameExists(dto.MuseumName);
    if (isMuseumNameExists)
    {
      return ErrorResp.BadRequest("Museum name already exists");
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

    // Index in Elasticsearch
    await _searchItemService.IndexMuseumAsync(museum.Id);

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

  public async Task<IActionResult> HandleBulkCreateUpdatePolicies(MuseumPolicyBulkRequestDto dto)
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

    if (dto.Policies == null || !dto.Policies.Any())
    {
      return ErrorResp.BadRequest("No policies provided");
    }

    var existingPolicies = _museumPolicyRepository.GetAllByMuseumId(dto.MuseumId);
    var policiesToProcess = new List<MuseumPolicy>();

    for (int i = 0; i < dto.Policies.Count; i++)
    {
      var policyDto = dto.Policies[i];
      var policy = _mapper.Map<MuseumPolicy>(policyDto);

      policy.MuseumId = dto.MuseumId;
      policy.ZOrder = i + 1; // Set ZOrder based on list position
      policy.CreatedBy = payload.UserId;

      if (policyDto.Id.HasValue && policyDto.Id != Guid.Empty)
      {
        policy.Id = policyDto.Id.Value;
      }
      else
      {
        policy.Id = Guid.NewGuid();
      }

      policiesToProcess.Add(policy);
    }

    var updatedPolicies = await _museumPolicyRepository.BulkCreateUpdateAsync(policiesToProcess, dto.MuseumId);

    // Delete policies that are no longer in the list
    var newPolicyIds = policiesToProcess.Select(p => p.Id).ToList();
    var policiesToDelete = existingPolicies
        .Where(p => !newPolicyIds.Contains(p.Id))
        .Select(p => p.Id)
        .ToList();

    if (policiesToDelete.Any())
    {
      await _museumPolicyRepository.DeleteByIdsAsync(policiesToDelete);
    }

    var resultDtos = _mapper.Map<IEnumerable<MuseumPolicyDto>>(updatedPolicies);
    return SuccessResp.Ok(new
    {
      List = resultDtos,
      Total = updatedPolicies.Count,
      DeletedCount = policiesToDelete.Count
    });
  }
}