namespace Application.Controllers;

using System;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Application.Service;
using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("/api/v1/museums")]
public class MuseumController : ControllerBase
{
  private readonly ILogger<MuseumController> _logger;
  private readonly IMuseumService _museumService;

  public MuseumController(ILogger<MuseumController> logger, IMuseumService museumService)
  {
    _logger = logger;
    _museumService = museumService;
  }

  [Protected]
  [HttpGet]
  public async Task<IActionResult> GetAllMuseums([FromQuery] MuseumQuery query)
  {
    _logger.LogInformation("Get all museums request received");
    return await _museumService.HandleGetAll(query);
  }

  [Protected]
  [HttpGet("{id}")]
  public async Task<IActionResult> GetMuseumById(Guid id)
  {
    _logger.LogInformation("Get museum by id request received");
    return await _museumService.HandleGetById(id);
  }

  [Protected]
  [HttpPost]
  public async Task<IActionResult> CreateMuseum([FromBody] MuseumCreateDto dto)
  {
    _logger.LogInformation("Create museum request received");
    return await _museumService.HandleCreate(dto);
  }

  [Protected]
  [HttpPut("{id}")]
  public async Task<IActionResult> UpdateMuseum(Guid id, [FromBody] MuseumUpdateDto dto)
  {
    _logger.LogInformation("Update museum request received");
    return await _museumService.HandleUpdate(id, dto);
  }

  [Protected]
  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteMuseum(Guid id)
  {
    _logger.LogInformation("Delete museum request received");
    return await _museumService.HandleDelete(id);
  }
}