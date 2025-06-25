namespace Application.Service;

using Application.DTOs.Plan;
using Application.Shared.Type;
using AutoMapper;
using Domain.Subscription;
using Database;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public interface ISubscriptionService
{
  Task<IActionResult> HandleGetAllAsync(PlanQuery query);
  Task<IActionResult> HandleGetByIdAsync(Guid id);
  Task<IActionResult> HandleCreateAsync(PlanCreateDto dto);
  Task<IActionResult> HandleUpdateAsync(Guid id, PlanUpdateDto dto);
  Task<IActionResult> HandleDeleteAsync(Guid id);
  Task<IActionResult> HandleGetAdminAsync();
}

public class SubscriptionService : BaseService, ISubscriptionService
{
  private readonly IPlanRepository _planRepository;

  public SubscriptionService(
      MuseTrip360DbContext dbContext,
      IMapper mapper,
      IHttpContextAccessor httpCtx)
      : base(dbContext, mapper, httpCtx)
  {
    _planRepository = new PlanRepository(dbContext);
  }

  public async Task<IActionResult> HandleGetAllAsync(PlanQuery query)
  {
    try
    {
      var plans = await _planRepository.GetAllAsync();
      var planDtos = _mapper.Map<List<PlanDto>>(plans);

      return SuccessResp.Ok(planDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving plans: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetByIdAsync(Guid id)
  {
    try
    {
      var plan = await _planRepository.GetByIdAsync(id);

      if (plan == null)
      {
        return ErrorResp.NotFound("Plan not found");
      }

      var planDto = _mapper.Map<PlanDto>(plan);
      return SuccessResp.Ok(planDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleCreateAsync(PlanCreateDto dto)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      // Check if name is unique
      var isNameUnique = await _planRepository.IsNameUniqueAsync(dto.Name);
      if (!isNameUnique)
      {
        return ErrorResp.BadRequest("Plan name already exists");
      }

      var plan = _mapper.Map<Plan>(dto);
      var createdPlan = await _planRepository.AddAsync(plan);

      var planDto = _mapper.Map<PlanDto>(createdPlan);
      return SuccessResp.Created(planDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error creating plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleUpdateAsync(Guid id, PlanUpdateDto dto)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var existingPlan = await _planRepository.GetByIdAsync(id);
      if (existingPlan == null)
      {
        return ErrorResp.NotFound("Plan not found");
      }

      // Check name uniqueness if name is being updated
      if (!string.IsNullOrEmpty(dto.Name) && dto.Name != existingPlan.Name)
      {
        var isNameUnique = await _planRepository.IsNameUniqueAsync(dto.Name, id);
        if (!isNameUnique)
        {
          return ErrorResp.BadRequest("Plan name already exists");
        }
      }

      _mapper.Map(dto, existingPlan);
      var updatedPlan = await _planRepository.UpdateAsync(existingPlan);

      var planDto = _mapper.Map<PlanDto>(updatedPlan);
      return SuccessResp.Ok(planDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error updating plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleDeleteAsync(Guid id)
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var plan = await _planRepository.GetByIdAsync(id);
      if (plan == null)
      {
        return ErrorResp.NotFound("Plan not found");
      }

      // Check if plan has active subscriptions
      var hasActiveSubscriptions = plan.Subscriptions
          .Any(s => s.Status == Application.Shared.Enum.SubscriptionStatusEnum.Active);

      if (hasActiveSubscriptions)
      {
        return ErrorResp.BadRequest("Cannot delete plan with active subscriptions");
      }

      await _planRepository.DeleteAsync(plan);
      return SuccessResp.NoContent();
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error deleting plan: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetAdminAsync()
  {
    try
    {
      var payload = ExtractPayload();
      if (payload == null)
      {
        return ErrorResp.Unauthorized("Invalid token");
      }

      var plans = await _planRepository.GetAllAsync();
      var planDtos = _mapper.Map<List<PlanDto>>(plans);

      var result = new
      {
        Plans = planDtos,
        TotalCount = planDtos.Count,
        ActiveCount = planDtos.Count(p => p.IsActive),
        InactiveCount = planDtos.Count(p => !p.IsActive)
      };

      return SuccessResp.Ok(result);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving admin plans: {ex.Message}");
    }
  }
}