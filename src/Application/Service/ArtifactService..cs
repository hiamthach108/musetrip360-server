using Application.Service;
using Application.Shared.Type;
using AutoMapper;
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
    // Task<IActionResult> HandleRate(Guid id, int rating);
    Task<IActionResult> HandleActivate(Guid id);
    Task<IActionResult> HandleDeactivate(Guid id);
}

public class ArtifactService : BaseService, IArtifactService
{
    private readonly IArtifactRepository _artifactRepository;
    private readonly IMuseumRepository _museumRepository;

    public ArtifactService(
        MuseTrip360DbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
        : base(dbContext, mapper, httpContextAccessor)
    {
        _artifactRepository = new ArtifactRepository(dbContext);
        _museumRepository = new MuseumRepository(dbContext);
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
            // map the dto to the artifact
            var artifact = _mapper.Map<Artifact>(dto);
            artifact.CreatedBy = payload.UserId;
            artifact.MuseumId = museumId;
            // create the artifact
            await _artifactRepository.AddAsync(artifact);
            var artifactDto = _mapper.Map<ArtifactDto>(artifact);
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
            return SuccessResp.Ok(artifactDtos);
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
            var isArtifactExists = await _artifactRepository.IsArtifactExistsAsync(id);
            if (!isArtifactExists)
            {
                return ErrorResp.NotFound("Artifact not found");
            }
            // update the artifact
            var artifact = _mapper.Map(dto, await _artifactRepository.GetByIdAsync(id));

            await _artifactRepository.UpdateAsync(id, artifact!);
            // return the success response
            return SuccessResp.Ok("Artifact updated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }

    // public async Task<IActionResult> HandleRate(Guid id, int rating)
    // {
    //     try
    //     {
    //         // check if the user is authenticated
    //         // check if the artifact is exists
    //         var isArtifactExists = await _artifactRepository.IsArtifactExistsAsync(id);
    //         if (!isArtifactExists)
    //         {
    //             return ErrorResp.NotFound("Artifact not found");
    //         }
    //         // rate the artifact
    //         var artifact = await _artifactRepository.GetByIdAsync(id);
    //         artifact!.Rating = rating;
    //         await _artifactRepository.UpdateAsync(id, artifact);
    //         // return the success response
    //         return SuccessResp.Ok("Artifact rated successfully");
    //     }
    //     catch (Exception e)
    //     {
    //         return ErrorResp.InternalServerError(e.Message);
    //     }
    // }

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
            artifact!.IsActive = true;
            await _artifactRepository.UpdateAsync(id, artifact);
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
            artifact!.IsActive = false;
            await _artifactRepository.UpdateAsync(id, artifact);
            // return the success response
            return SuccessResp.Ok("Artifact deactivated successfully");
        }
        catch (Exception e)
        {
            return ErrorResp.InternalServerError(e.Message);
        }
    }
}
