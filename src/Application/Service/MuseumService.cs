namespace Application.Service;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Museums;
using Domain.Museums;
using Infrastructure.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Application.Shared.Type;
using Database;

public interface IMuseumService
{
  Task<IActionResult> HandleGetAllAsync();
  Task<IActionResult> HandleGetByIdAsync(Guid id);
  Task<IActionResult> HandleCreateAsync(MuseumCreateDto dto);
  Task<IActionResult> HandleUpdateAsync(Guid id, MuseumUpdateDto dto);
  Task<IActionResult> HandleDeleteAsync(Guid id);
}

public class MuseumService : BaseService, IMuseumService
{
  private readonly IMuseumRepository _museumRepository;

  public MuseumService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpCtx)
    : base(dbContext, mapper, httpCtx)
  {
    _museumRepository = new MuseumRepository(dbContext);
  }

  public async Task<IActionResult> HandleGetAllAsync()
  {
    var museums = _museumRepository.GetAllAsync();
    var museumDtos = _mapper.Map<IEnumerable<MuseumDto>>(museums);
    return SuccessResp.Ok(museumDtos);
  }

  public async Task<IActionResult> HandleGetByIdAsync(Guid id)
  {
    var museum = _museumRepository.GetById(id);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }
    var museumDto = _mapper.Map<MuseumDto>(museum);
    return SuccessResp.Ok(museumDto);
  }

  public async Task<IActionResult> HandleCreateAsync(MuseumCreateDto dto)
  {
    var payload = ExtractPayload();
    if (payload == null)
    {
      return ErrorResp.Unauthorized("Invalid token");
    }

    var museum = _mapper.Map<Museum>(dto);
    museum.CreatedBy = payload.UserId;
    await _museumRepository.AddAsync(museum);
    return SuccessResp.Created("Museum created successfully");
  }

  public async Task<IActionResult> HandleUpdateAsync(Guid id, MuseumUpdateDto dto)
  {
    var museum = _museumRepository.GetById(id);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }
    _mapper.Map(dto, museum);
    await _museumRepository.UpdateAsync(id, museum);
    return SuccessResp.Ok("Museum updated successfully");
  }

  public async Task<IActionResult> HandleDeleteAsync(Guid id)
  {
    var museum = _museumRepository.GetById(id);
    if (museum == null)
    {
      return ErrorResp.NotFound("Museum not found");
    }
    await _museumRepository.DeleteAsync(museum);
    return SuccessResp.Ok("Museum deleted successfully");
  }
}