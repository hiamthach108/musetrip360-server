using Application.Middlewares;
using Application.Service;
using Application.Shared.Enum;
using Microsoft.AspNetCore.Mvc;
using MuseTrip360.src.Application.Service;

namespace Application.Controllers;

[ApiController]
[Route("/api/v1/feedbacks")]
[Produces("application/json")]
public class FeedbackController : ControllerBase
{
    private readonly IArtifactService _artifactService;
    private readonly IMuseumService _museumService;
    private readonly IEventService _eventService;
    private readonly ITourOnlineService _tourOnlineService;

    public FeedbackController(IArtifactService artifactService, IMuseumService museumService, IEventService eventService, ITourOnlineService tourOnlineService)
    {
        _artifactService = artifactService;
        _museumService = museumService;
        _eventService = eventService;
        _tourOnlineService = tourOnlineService;
    }

    /// <summary>
    /// Create feedback for an artifact, museum, event or online tour
    /// Rating for event, tour online will be ignore
    /// </summary>
    /// <param name="targetId">The unique identifier of the artifact, museum, event or online tour</param>
    /// <param name="dto">The feedback data</param>
    /// <returns>The created feedback</returns>
    /// <response code="200">Returns the created feedback</response>
    /// <response code="400">Returns an error message if the type is invalid</response>
    [Protected]
    [HttpPost("{targetId}/target")]
    public async Task<IActionResult> CreateFeedback([FromRoute] Guid targetId, [FromBody] FeedbackCreateDto dto)
    {
        switch (dto.Target)
        {
            case DataEntityType.Artifact:
                return await _artifactService.HandleFeedback(targetId, dto.Rating, dto.Comment);
            case DataEntityType.Museum:
                return await _museumService.HandleFeedback(targetId, dto.Rating, dto.Comment);
            case DataEntityType.Event:
                return await _eventService.HandleFeedback(targetId, dto.Comment);
            case DataEntityType.TourOnline:
                return await _tourOnlineService.HandleFeedback(targetId, dto.Comment);
            default:
                return BadRequest("Invalid type");
        }
    }
    [Protected]
    [HttpPost("{targetId}/target/get")]
    public async Task<IActionResult> GetFeedback([FromRoute] Guid targetId, [FromBody] DataEntityType target)
    {
        switch (target)
        {
            case DataEntityType.Artifact:
                return await _artifactService.HandleGetFeedbackByArtifactId(targetId);
            case DataEntityType.Museum:
                return await _museumService.HandleGetFeedbackByMuseumId(targetId);
            case DataEntityType.Event:
                return await _eventService.HandleGetFeedbackByEventId(targetId);
            case DataEntityType.TourOnline:
                return await _tourOnlineService.HandleGetFeedbackByTourOnlineId(targetId);
            default:
                return BadRequest("Invalid type");
        }
    }
}
