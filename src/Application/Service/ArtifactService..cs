using Application.DTOs.Feedback;
using Application.DTOs.Search;
using Application.Service;
using Application.Shared.Constant;
using Application.Shared.Type;
using AutoMapper;
using Core.Queue;
using Database;
using Domain.Artifacts;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using MuseTrip360.src.Application.DTOs.Artifact;
using MuseTrip360.src.Infrastructure.Repository;

namespace MuseTrip360.src.Application.Service;

public interface IArtifactService
{
    Task<IActionResult> HandleGetAll(ArtifactQuery query);
    Task<IActionResult> HandleGetAllAdmin(ArtifactAdminQuery query);
    Task<IActionResult> HandleGetById(Guid id);
    Task<IActionResult> HandleGetByMuseumId(Guid museumId, ArtifactAdminQuery query);
    Task<IActionResult> HandleCreate(Guid museumId, ArtifactCreateDto dto);
    Task<IActionResult> HandleUpdate(Guid id, ArtifactUpdateDto dto);
    Task<IActionResult> HandleDelete(Guid id);
    Task<IActionResult> HandleActivate(Guid id);
    Task<IActionResult> HandleDeactivate(Guid id);
    Task<IActionResult> HandleGetByFilterSort(ArtifactFilterSort filterSort);
    Task<IActionResult> HandleFeedback(Guid id, int rating, string comment);
    Task<IActionResult> HandleGetFeedbackByArtifactId(Guid id);
}

public class ArtifactService : BaseService, IArtifactService
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly IMuseumRepository _museumRepository;
    private readonly IQueuePublisher _queuePub;
    private readonly IUserService _userSvc;


    public ArtifactService(
        MuseTrip360DbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IQueuePublisher queuePub,
        IUserService userSvc
    )
        : base(dbContext, mapper, httpContextAccessor)
    {
        _artifactRepository = new ArtifactRepository(dbContext);
        _museumRepository = new MuseumRepository(dbContext);
        _queuePub = queuePub;
        _userSvc = userSvc;
    }

    public async Task<IActionResult> HandleCreate(Guid museumId, ArtifactCreateDto dto)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            // check if the museum is exists
            var isMuseumExists = _museumRepository.IsMuseumExists(museumId);
            if (!isMuseumExists)
            {
                return ErrorResp.NotFound("Museum not found");
            }
            var isAllowed = await _userSvc.ValidatePermission(museumId.ToString(), [PermissionConst.ARTIFACTS_MANAGEMENT]);
            if (!isAllowed)
            {
                return ErrorResp.Forbidden("You are not allowed to access this resource");
            }
            // map the dto to the artifact
            var artifact = _mapper.Map<Artifact>(dto);
            artifact.CreatedBy = payload.UserId;
            artifact.MuseumId = museumId;
            // create the artifact
            await _artifactRepository.AddAsync(artifact);
            var artifactDto = _mapper.Map<ArtifactDto>(artifact);

            // publish the artifact created event
            // push to queue for index data
            await _queuePub.Publish(QueueConst.Indexing, new IndexMessage
            {
                Id = artifactDto.Id,
                Type = IndexConst.ARTIFACT_TYPE,
                Action = IndexConst.CREATE_ACTION
            });

            return SuccessResp.Created(artifactDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAll(ArtifactQuery query)
    {
        try
        {
            // get all artifacts
            var artifacts = await _artifactRepository.GetAllAsync(query);
            // map the artifacts to the artifact dtos
            var artifactDtos = _mapper.Map<List<ArtifactDto>>(artifacts.Artifacts);
            // return the artifact dtos
            return SuccessResp.Ok(new
            {
                List = artifactDtos,
                Total = artifacts.Total
            });
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
            // check if the artifact is exists
            var isArtifactExists = await _artifactRepository.IsArtifactExistsAsync(id);
            if (!isArtifactExists)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            // delete the artifact
            await _artifactRepository.DeleteAsync(id);

            // publish the artifact deleted event
            // push to queue for index data
            await _queuePub.Publish(QueueConst.Indexing, new IndexMessage
            {
                Id = id,
                Type = IndexConst.ARTIFACT_TYPE,
                Action = IndexConst.DELETE_ACTION
            });
            // return the success response
            return SuccessResp.Ok("Artifact deleted successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetAllAdmin(ArtifactAdminQuery query)
    {
        try
        {
            // get all artifacts
            var artifacts = await _artifactRepository.GetAllAdminAsync(query);
            // map the artifacts to the artifact dtos
            var artifactDtos = _mapper.Map<List<ArtifactDto>>(artifacts.Artifacts);
            // return the artifact dtos
            return SuccessResp.Ok(new
            {
                List = artifactDtos,
                Total = artifacts.Total
            });
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
            // get the artifact
            var artifact = await _artifactRepository.GetByIdAsync(id);
            if (artifact == null)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            // map the artifact to the artifact dto
            var artifactDto = _mapper.Map<ArtifactDto>(artifact);
            // return the artifact dto
            return SuccessResp.Ok(artifactDto);
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByMuseumId(Guid museumId, ArtifactAdminQuery query)
    {
        try
        {
            // get the artifacts
            var artifacts = await _artifactRepository.GetByMuseumIdAsync(museumId, query);
            // map the artifacts to the artifact dtos
            var artifactDtos = _mapper.Map<List<ArtifactDto>>(artifacts.Artifacts);
            // return the artifact dtos
            return SuccessResp.Ok(new
            {
                List = artifactDtos,
                Total = artifacts.Total
            });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleUpdate(Guid id, ArtifactUpdateDto dto)
    {
        try
        {
            // check if the artifact is exists
            var existed = await _artifactRepository.GetByIdAsync(id);
            if (existed == null)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            var isAllowed = await _userSvc.ValidatePermission(existed.MuseumId.ToString(), [PermissionConst.ARTIFACTS_MANAGEMENT]);
            if (!isAllowed)
            {
                return ErrorResp.Forbidden("You are not allowed to access this resource");
            }
            // update the artifact
            var artifact = _mapper.Map(dto, existed);

            await _artifactRepository.UpdateAsync(id, artifact!);
            // publish the artifact updated event
            // push to queue for index data
            await _queuePub.Publish(QueueConst.Indexing, new IndexMessage
            {
                Id = id,
                Type = IndexConst.ARTIFACT_TYPE,
                Action = IndexConst.CREATE_ACTION
            });
            // return the success response
            return SuccessResp.Ok("Artifact updated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleActivate(Guid id)
    {
        try
        {
            // check if the artifact is exists
            var isArtifactExists = await _artifactRepository.IsArtifactExistsAsync(id);
            if (!isArtifactExists)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            // activate the artifact
            var artifact = await _artifactRepository.GetByIdAsync(id);
            if (artifact == null)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            var isAllowed = await _userSvc.ValidatePermission(artifact.MuseumId.ToString(), [PermissionConst.ARTIFACTS_MANAGEMENT]);
            if (!isAllowed)
            {
                return ErrorResp.Forbidden("You are not allowed to access this resource");
            }
            artifact!.IsActive = true;
            await _artifactRepository.UpdateAsync(id, artifact);
            // publish the artifact activated event
            // push to queue for index data
            await _queuePub.Publish(QueueConst.Indexing, new IndexMessage
            {
                Id = id,
                Type = IndexConst.ARTIFACT_TYPE,
                Action = IndexConst.CREATE_ACTION
            });
            // return the success response
            return SuccessResp.Ok("Artifact activated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleDeactivate(Guid id)
    {
        try
        {
            // check if the artifact is exists
            var isArtifactExists = await _artifactRepository.IsArtifactExistsAsync(id);
            if (!isArtifactExists)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            // deactivate the artifact
            var artifact = await _artifactRepository.GetByIdAsync(id);
            if (artifact == null)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            var isAllowed = await _userSvc.ValidatePermission(artifact.MuseumId.ToString(), [PermissionConst.ARTIFACTS_MANAGEMENT]);
            if (!isAllowed)
            {
                return ErrorResp.Forbidden("You are not allowed to access this resource");
            }
            artifact!.IsActive = false;
            await _artifactRepository.UpdateAsync(id, artifact);
            // publish the artifact deactivated event
            // push to queue for index data
            await _queuePub.Publish(QueueConst.Indexing, new IndexMessage
            {
                Id = id,
                Type = IndexConst.ARTIFACT_TYPE,
                Action = IndexConst.CREATE_ACTION
            });
            // return the success response
            return SuccessResp.Ok("Artifact deactivated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetByFilterSort(ArtifactFilterSort filterSort)
    {
        try
        {
            // get the artifacts
            var artifacts = await _artifactRepository.GetArtifactByFilterSort(filterSort);
            // map the artifacts to the artifact dtos
            var artifactDtos = _mapper.Map<List<ArtifactDto>>(artifacts.Artifacts);
            // return the artifact dtos
            return SuccessResp.Ok(new
            {
                List = artifactDtos,
                Total = artifacts.Total
            });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleFeedback(Guid id, int rating, string comment)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            // check if the artifact is exists
            var isArtifactExists = await _artifactRepository.IsArtifactExistsAsync(id);
            if (!isArtifactExists)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            // rate the artifact
            await _artifactRepository.FeedbackArtifacts(id, rating, payload.UserId, comment);
            // return the success response
            return SuccessResp.Ok("Rating artifact successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    public async Task<IActionResult> HandleGetFeedbackByArtifactId(Guid id)
    {
        try
        {
            var feedback = await _artifactRepository.GetFeedbackByArtifactIdAsync(id);
            var feedbackDtos = _mapper.Map<IEnumerable<FeedbackDto>>(feedback);
            return SuccessResp.Ok(new
            {
                List = feedbackDtos,
                Total = feedbackDtos.Count()
            });
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}