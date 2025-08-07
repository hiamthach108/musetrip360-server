namespace Application.Service;

using Application.DTOs.HistoricalPeriod;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Content;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface IHistoricalPeriodService
{
  Task<IActionResult> HandleGetAllAsync();
  Task<IActionResult> HandleGetByIdAsync(Guid id);
  Task<IActionResult> HandleCreateAsync(HistoricalPeriodCreateDto dto);
  Task<IActionResult> HandleUpdateAsync(Guid id, HistoricalPeriodUpdateDto dto);
  Task<IActionResult> HandleDeleteAsync(Guid id);
  Task<IActionResult> HandleSearchAsync(HistoricalPeriodQueryDto query);
}

public class HistoricalPeriodService : BaseService, IHistoricalPeriodService
{
  private readonly IHistoricalPeriodRepository _historicalPeriodRepository;

  public HistoricalPeriodService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpCtx)
    : base(dbContext, mapper, httpCtx)
  {
    _historicalPeriodRepository = new HistoricalPeriodRepository(dbContext);
  }

  public async Task<IActionResult> HandleGetAllAsync()
  {
    try
    {
      var historicalPeriods = await _historicalPeriodRepository.GetAllAsync();
      var historicalPeriodDtos = _mapper.Map<IEnumerable<HistoricalPeriodDto>>(historicalPeriods);
      return SuccessResp.Ok(historicalPeriodDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving historical periods: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleGetByIdAsync(Guid id)
  {
    try
    {
      var historicalPeriod = await _historicalPeriodRepository.GetByIdAsync(id);
      if (historicalPeriod == null)
        return ErrorResp.NotFound("Historical period not found");

      var historicalPeriodDto = _mapper.Map<HistoricalPeriodDto>(historicalPeriod);
      return SuccessResp.Ok(historicalPeriodDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error retrieving historical period: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleCreateAsync(HistoricalPeriodCreateDto dto)
  {
    try
    {
      if (await _historicalPeriodRepository.NameExistsAsync(dto.Name))
        return ErrorResp.BadRequest("Historical period name already exists");

      var historicalPeriod = _mapper.Map<HistoricalPeriod>(dto);
      var createdHistoricalPeriod = await _historicalPeriodRepository.AddAsync(historicalPeriod);
      var historicalPeriodDto = _mapper.Map<HistoricalPeriodDto>(createdHistoricalPeriod);

      return SuccessResp.Created(historicalPeriodDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error creating historical period: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleUpdateAsync(Guid id, HistoricalPeriodUpdateDto dto)
  {
    try
    {
      if (!await _historicalPeriodRepository.ExistsAsync(id))
        return ErrorResp.NotFound("Historical period not found");

      if (await _historicalPeriodRepository.NameExistsAsync(dto.Name, id))
        return ErrorResp.BadRequest("Historical period name already exists");

      var historicalPeriod = _mapper.Map<HistoricalPeriod>(dto);
      var updatedHistoricalPeriod = await _historicalPeriodRepository.UpdateAsync(id, historicalPeriod);

      if (updatedHistoricalPeriod == null)
        return ErrorResp.NotFound("Historical period not found");

      var historicalPeriodDto = _mapper.Map<HistoricalPeriodDto>(updatedHistoricalPeriod);
      return SuccessResp.Ok(historicalPeriodDto);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error updating historical period: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleDeleteAsync(Guid id)
  {
    try
    {
      var deleted = await _historicalPeriodRepository.DeleteAsync(id);
      if (!deleted)
        return ErrorResp.NotFound("Historical period not found");

      return SuccessResp.NoContent();
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error deleting historical period: {ex.Message}");
    }
  }

  public async Task<IActionResult> HandleSearchAsync(HistoricalPeriodQueryDto query)
  {
    try
    {
      var historicalPeriods = await _historicalPeriodRepository.SearchByNameAsync(query.Name);
      var historicalPeriodDtos = _mapper.Map<IEnumerable<HistoricalPeriodDto>>(historicalPeriods);
      return SuccessResp.Ok(historicalPeriodDtos);
    }
    catch (Exception ex)
    {
      return ErrorResp.InternalServerError($"Error searching historical periods: {ex.Message}");
    }
  }
}