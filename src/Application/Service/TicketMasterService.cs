using Application.Service;
using Application.Shared.Type;
using AutoMapper;
using Database;
using Domain.Tickets;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface ITicketMasterService
{
    Task<IActionResult> GetTicketMasterQuery(TicketMasterQuery query);
    Task<IActionResult> GetTicketMasterById(Guid id);
    Task<IActionResult> CreateTicketMaster(Guid museumId, TicketMasterCreateDto ticketMaster);
    Task<IActionResult> UpdateTicketMaster(Guid id, TicketMasterUpdateDto ticketMaster);
    Task<IActionResult> DeleteTicketMaster(Guid id);
    Task<IActionResult> GetTicketMasterByMuseumId(Guid museumId);

}

public class TicketMasterService : BaseService, ITicketMasterService
{
    protected readonly ITicketMasterRepository _ticketMasterRepository;
    protected readonly IMuseumRepository _museumRepository;

    public TicketMasterService(MuseTrip360DbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(context, mapper, httpContextAccessor)
    {
        _ticketMasterRepository = new TicketMasterRepository(context);
        _museumRepository = new MuseumRepository(context);
    }

    public async Task<IActionResult> CreateTicketMaster(Guid museumId, TicketMasterCreateDto ticketMaster)
    {
        try
        {
            if (!_museumRepository.IsMuseumExists(museumId))
            {
                return ErrorResp.NotFound("Museum not found"); // TODO: Create custom exception
            }
            var ticketMasterToCreate = _mapper.Map<TicketMaster>(ticketMaster);
            ticketMasterToCreate.MuseumId = museumId;
            await _ticketMasterRepository.CreateTicketMaster(ticketMasterToCreate);
            var mappedTicketMaster = _mapper.Map<TicketMasterDto>(ticketMasterToCreate);
            return SuccessResp.Created(mappedTicketMaster);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> DeleteTicketMaster(Guid id)
    {
        try
        {
            if (await _ticketMasterRepository.GetTicketMasterById(id) == null)
            {
                return ErrorResp.NotFound("Ticket master not found");
            }
            await _ticketMasterRepository.DeleteTicketMaster(id);
            return SuccessResp.Ok("Ticket master deleted successfully");
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> GetTicketMasterById(Guid id)
    {
        try
        {
            var ticketMaster = await _ticketMasterRepository.GetTicketMasterById(id);
            if (ticketMaster == null)
            {
                return ErrorResp.NotFound("Ticket master not found");
            }
            var mappedTicketMaster = _mapper.Map<TicketMasterDto>(ticketMaster);
            return SuccessResp.Ok(mappedTicketMaster);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> GetTicketMasterByMuseumId(Guid museumId)
    {
        try
        {
            var ticketMasters = await _ticketMasterRepository.GetTicketMasterByMuseumId(museumId);
            var mappedTicketMasters = _mapper.Map<List<TicketMasterDto>>(ticketMasters);
            return SuccessResp.Ok(mappedTicketMasters);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> GetTicketMasterQuery(TicketMasterQuery query)
    {
        try
        {
            var ticketMasters = await _ticketMasterRepository.GetTicketMasterQuery(query);
            var mappedTicketMasters = _mapper.Map<List<TicketMasterDto>>(ticketMasters.TicketMasters);
            return SuccessResp.Ok(mappedTicketMasters);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> UpdateTicketMaster(Guid id, TicketMasterUpdateDto dto)
    {
        try
        {
            var ticketMaster = await _ticketMasterRepository.GetTicketMasterById(id);
            if (ticketMaster == null)
            {
                return ErrorResp.NotFound("Ticket master not found");
            }
            var ticketMasterToUpdate = _mapper.Map(dto, ticketMaster);
            await _ticketMasterRepository.UpdateTicketMaster(id, ticketMasterToUpdate);
            var mappedTicketMaster = _mapper.Map<TicketMasterDto>(ticketMasterToUpdate);
            return SuccessResp.Ok(mappedTicketMaster);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }
}