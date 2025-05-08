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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return await _artifactService.HandleGetById(id);
    }

    [HttpPost("{museumId}")]
    public async Task<IActionResult> Create(Guid museumId, ArtifactCreateDto dto)
    {
        return await _artifactService.HandleCreate(museumId, dto);
    }   

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, ArtifactUpdateDto dto)
    {
        return await _artifactService.HandleUpdate(id, dto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await _artifactService.HandleDelete(id);
    }
}
