using Application.Middlewares;
using Microsoft.AspNetCore.Mvc;
using MuseTrip360.src.Application.DTOs.Artifact;
using MuseTrip360.src.Application.Service;

namespace MuseTrip360.src.Application.Controllers;

[Route("api/v1/artifacts")]
[ApiController]
public class ArtifactController : ControllerBase
{
    private readonly IArtifactService _artifactService;

    public ArtifactController(IArtifactService artifactService)
    {
        _artifactService = artifactService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ArtifactQuery query)
    {
        return await _artifactService.HandleGetAll(query);
    }
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllAdmin([FromQuery] ArtifactAdminQuery query)
    {
        return await _artifactService.HandleGetAllAdmin(query);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _artifactService.HandleGetById(id);
    }
    [HttpGet("museum/{museumId}/artifacts")]
    public async Task<IActionResult> GetByMuseumId(Guid museumId)
    {
        return await _artifactService.HandleGetByMuseumId(museumId);
    }
    [Protected]
    [HttpPost("{museumId}")]
    public async Task<IActionResult> Create(Guid museumId, ArtifactCreateDto dto)
    {
        return await _artifactService.HandleCreate(museumId, dto);
    }   

    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, ArtifactUpdateDto dto)
    {
        return await _artifactService.HandleUpdate(id, dto);
    }
    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _artifactService.HandleDelete(id);
    }
    // [Protected]
    // [HttpPost("{id}/rate")]
    // public async Task<IActionResult> Rate(Guid id, int rating)
    // {
    //     return await _artifactService.HandleRate(id, rating);
    // }
    [Protected]
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {   
        return await _artifactService.HandleActivate(id);
    }
    [Protected]
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        return await _artifactService.HandleDeactivate(id);
    }
}
