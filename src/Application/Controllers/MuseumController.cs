namespace Application.Controllers;

using System;
using System.Threading.Tasks;
using Application.DTOs.Museum;
using Application.Service;
using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs.MuseumRequest;
using Application.DTOs.Pagination;
using Application.DTOs.MuseumPolicy;

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

  [HttpGet]
  public async Task<IActionResult> GetAllMuseums([FromQuery] MuseumQuery query)
  {
    _logger.LogInformation("Get all museums request received");
    return await _museumService.HandleGetAll(query);
  }

  [Protected]
  [HttpGet("admin")]
  public async Task<IActionResult> GetAllMuseumsAdmin([FromQuery] MuseumQuery query)
  {
    _logger.LogInformation("Get all museums request received");
    return await _museumService.HandleGetAllAdmin(query);
  }

  [Protected]
  [HttpGet("user")]
  public async Task<IActionResult> GetUserMuseums()
  {
    _logger.LogInformation("Get user museums request received");
    return await _museumService.HandleGetUserMuseums();
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

  // MuseumRequest endpoints
  [Protected]
  [HttpGet("requests")]
  public async Task<IActionResult> GetAllMuseumRequests([FromQuery] MuseumRequestQuery query)
  {
    _logger.LogInformation("Get all museum requests received");
    return await _museumService.HandleGetAllRequests(query);
  }

  [Protected]
  [HttpGet("requests/user")]
  public async Task<IActionResult> GetAllMuseumRequestsByUserId([FromQuery] MuseumRequestQuery query)
  {
    _logger.LogInformation("Get all museum requests by user id received");
    return await _museumService.HandleGetAllRequestsByUserId(query);
  }

  [Protected]
  [HttpGet("requests/{id}")]
  public async Task<IActionResult> GetMuseumRequestById(Guid id)
  {
    _logger.LogInformation("Get museum request by id received");
    return await _museumService.HandleGetRequestById(id);
  }

  [Protected]
  [HttpPost("requests")]
  public async Task<IActionResult> CreateMuseumRequest([FromBody] MuseumRequestCreateDto dto)
  {
    _logger.LogInformation("Create museum request received");
    return await _museumService.HandleCreateRequest(dto);
  }

  [Protected]
  [HttpPut("requests/{id}")]
  public async Task<IActionResult> UpdateMuseumRequest(Guid id, [FromBody] MuseumRequestUpdateDto dto)
  {
    _logger.LogInformation("Update museum request received");
    return await _museumService.HandleUpdateRequest(id, dto);
  }

  [Protected]
  [HttpDelete("requests/{id}")]
  public async Task<IActionResult> DeleteMuseumRequest(Guid id)
  {
    _logger.LogInformation("Delete museum request received");
    return await _museumService.HandleDeleteRequest(id);
  }

  [Protected]
  [HttpPatch("requests/{id}/approve")]
  public async Task<IActionResult> ApproveMuseumRequest(Guid id)
  {
    _logger.LogInformation("Approve museum request received");
    return await _museumService.HandleApproveRequest(id);
  }

  [Protected]
  [HttpPatch("requests/{id}/reject")]
  public async Task<IActionResult> RejectMuseumRequest(Guid id)
  {
    _logger.LogInformation("Reject museum request received");
    return await _museumService.HandleRejectRequest(id);
  }

  // MuseumPolicy endpoints
  [Protected]
  [HttpGet("policies/museum/{museumId}")]
  public async Task<IActionResult> GetAllMuseumPolicies(Guid museumId, [FromQuery] PaginationReq query)
  {
    _logger.LogInformation("Get all museum policies received");
    return await _museumService.HandleGetAllPolicies(query, museumId);
  }

  [Protected]
  [HttpGet("policies/{id}")]
  public async Task<IActionResult> GetMuseumPolicyById(Guid id)
  {
    _logger.LogInformation("Get museum policy by id received");
    return await _museumService.HandleGetPolicyById(id);
  }

  [Protected]
  [HttpPost("policies")]
  public async Task<IActionResult> CreateMuseumPolicy([FromBody] MuseumPolicyCreateDto dto)
  {
    _logger.LogInformation("Create museum policy received");
    return await _museumService.HandleCreatePolicy(dto);
  }

  [Protected]
  [HttpPut("policies/{id}")]
  public async Task<IActionResult> UpdateMuseumPolicy(Guid id, [FromBody] MuseumPolicyUpdateDto dto)
  {
    _logger.LogInformation("Update museum policy received");
    return await _museumService.HandleUpdatePolicy(id, dto);
  }

  [Protected]
  [HttpDelete("policies/{id}")]
  public async Task<IActionResult> DeleteMuseumPolicy(Guid id)
  {
    _logger.LogInformation("Delete museum policy received");
    return await _museumService.HandleDeletePolicy(id);
  }
}