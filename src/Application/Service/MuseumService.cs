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

public interface IMuseumService
{
  Task<IActionResult> HandleGetAll(MuseumQuery query);
  Task<IActionResult> HandleGetById(Guid id);
  Task<IActionResult> HandleCreate(MuseumCreateDto dto);
  Task<IActionResult> HandleUpdate(Guid id, MuseumUpdateDto dto);
  Task<IActionResult> HandleDelete(Guid id);
}

public class MuseumService : BaseService, IMuseumService
{
  private readonly IMuseumRepository _museumRepository;

  public MuseumService(MuseTrip360DbContext dbContext, IMapper mapper, IHttpContextAccessor httpCtx)
    : base(dbContext, mapper, httpCtx)
  {
    _museumRepository = new MuseumRepository(dbContext);
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
}