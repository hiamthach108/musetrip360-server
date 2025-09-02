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
    [HttpPost("")]
    public async Task<IActionResult> CreateFeedback([FromBody] FeedbackCreateDto dto)
    {
        switch (dto.Target)
        {
            case DataEntityType.Artifact:
                return await _artifactService.HandleFeedback(dto.TargetId, dto.Rating, dto.Comment);
            case DataEntityType.Museum:
                return await _museumService.HandleFeedback(dto.TargetId, dto.Rating, dto.Comment);
            case DataEntityType.Event:
                return await _eventService.HandleFeedback(dto.TargetId, dto.Comment, dto.Rating);
            case DataEntityType.TourOnline:
                return await _tourOnlineService.HandleFeedback(dto.TargetId, dto.Comment);
            default:
                return BadRequest("Invalid type");
        }
    }
    [Protected]
    [HttpGet("")]
    public async Task<IActionResult> GetFeedback([FromQuery] FeedbackQuery query)
    {
        switch (query.Type)
        {
            case DataEntityType.Artifact:
                return await _artifactService.HandleGetFeedbackByArtifactId(query.TargetId);
            case DataEntityType.Museum:
                return await _museumService.HandleGetFeedbackByMuseumId(query.TargetId);
            case DataEntityType.Event:
                return await _eventService.HandleGetFeedbackByEventId(query.TargetId);
            case DataEntityType.TourOnline:
                return await _tourOnlineService.HandleGetFeedbackByTourOnlineId(query.TargetId);
            default:
                return BadRequest("Invalid type");
        }
    }
}
