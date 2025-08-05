using System.Reflection.Metadata.Ecma335;
using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Content;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.RepresentationMaterial;

public interface IRepresentationMaterialService
{
    Task<IActionResult> HandleGetAll();
    Task<IActionResult> HandleGetById(Guid id);
    Task<IActionResult> HandleCreate(Guid eventId, RepresentationMaterialCreateDto dto);
    Task<IActionResult> HandleUpdate(Guid id, RepresentationMaterialUpdateDto dto);
    Task<IActionResult> HandleDelete(Guid id);
}

public class RepresentationMaterialService : BaseService, IRepresentationMaterialService
{
    private readonly IRepresentationMaterialRepository _representationMaterialRepository;
    private readonly IEventRepository _eventRepository;

    public RepresentationMaterialService(
        MuseTrip360DbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : base(dbContext, mapper, httpContextAccessor)
    {
        _representationMaterialRepository = new RepresentationMaterialRepository(dbContext);
        _eventRepository = new EventRepository(dbContext);
    }
    public async Task<IActionResult> HandleCreate(Guid eventId, RepresentationMaterialCreateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            // check if event exist
            var eventExist = await _eventRepository.IsEventExistsAsync(eventId);
            if (!eventExist)
            {
                return ErrorResp.NotFound("Event not found");
            }
            // create representation material
            var representationMaterial = _mapper.Map<RepresentationMaterial>(dto);
            representationMaterial.EventId = eventId;
            representationMaterial.CreatedBy = payload.UserId;
            // create representation material
            await _representationMaterialRepository.AddAsync(representationMaterial);
            var representationMaterialDto = _mapper.Map<RepresentationMaterialDto>(representationMaterial);
            return SuccessResp.Created(representationMaterialDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDelete(Guid id)
    {
        try
        {
            // check if representation material exist
            var representationMaterialExist = await _representationMaterialRepository.GetByIdAsync(id);
            if (representationMaterialExist == null)
            {
                return ErrorResp.NotFound("Representation material not found");
            }
            // delete representation material
            await _representationMaterialRepository.DeleteAsync(representationMaterialExist);
            return SuccessResp.Ok("Representation material deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAll()
    {
        try
        {
            var representationMaterials = await _representationMaterialRepository.GetAllAsync();
            var representationMaterialDtos = _mapper.Map<IEnumerable<RepresentationMaterialDto>>(representationMaterials);
            return SuccessResp.Ok(representationMaterialDtos);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetById(Guid id)
    {
        try
        {
            // get representation material
            var representationMaterial = await _representationMaterialRepository.GetByIdAsync(id);
            if (representationMaterial == null)
            {
                return ErrorResp.NotFound("Representation material not found");
            }
            var representationMaterialDto = _mapper.Map<RepresentationMaterialDto>(representationMaterial);
            return SuccessResp.Ok(representationMaterialDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdate(Guid id, RepresentationMaterialUpdateDto dto)
    {
        try
        {
            // check if representation material exist
            var representationMaterialExist = await _representationMaterialRepository.GetByIdAsync(id);
            if (representationMaterialExist == null)
            {
                return ErrorResp.NotFound("Representation material not found");
            }
            // update representation material
            var representationMaterial = _mapper.Map(dto, representationMaterialExist);
            await _representationMaterialRepository.UpdateAsync(id, representationMaterial);
            var representationMaterialDto = _mapper.Map<RepresentationMaterialDto>(representationMaterial);
            return SuccessResp.Ok(representationMaterialDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}
