using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface IEventService
{
    Task<IActionResult> HandleGetEventById(Guid id);
    Task<IActionResult> HandleGetAllAsync(EventQuery query);
    Task<IActionResult> HandleGetAllAdminAsync(EventAdminQuery query);
    Task<IActionResult> HandleCreate(Guid museumId, EventCreateDto dto);
    Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto);
    Task<IActionResult> HandleDelete(Guid id);
}

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
}

    public Task<IActionResult> HandleCreate(Guid museumId, EventCreateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> HandleDelete(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> HandleGetAllAdminAsync(EventAdminQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> HandleGetAllAsync(EventQuery query)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> HandleGetEventById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IActionResult> HandleUpdate(Guid id, EventUpdateDto dto)
    {
        throw new NotImplementedException();
    }
}
