using System.Diagnostics.Eventing.Reader;
using Database;
using Domain.Artifacts;
using Microsoft.EntityFrameworkCore;
using MuseTrip360.src.Application.DTOs.Artifact;

namespace MuseTrip360.src.Infrastructure.Repository
{
    public interface IArtifactRepository
    {
        Task<Artifact?> GetByIdAsync(Guid id);
        Task<IEnumerable<Artifact>> GetByMuseumIdAsync(Guid museumId);
        Task<ArtifactList> GetAllAsync(ArtifactQuery query);
        Task<ArtifactList> GetAllAdminAsync(ArtifactAdminQuery query);
        Task AddAsync(Artifact artifact);
        Task UpdateAsync(Guid artifactId, Artifact artifact);
        Task DeleteAsync(Guid id);
        Task<bool> IsMuseumExistsAsync(Guid museumId);
        Task<bool> IsArtifactExistsAsync(Guid artifactId);
        Task<ArtifactListResultWithMissingIds> GetArtifactByListIdMuseumIdStatus(List<Guid> artifactIds, Guid museumId, bool status);
        Task<ArtifactListResultWithMissingIds> GetArtifactByListIdEventId(List<Guid> artifactIds, Guid eventId);
    }
    public class ArtifactList
    {
        public IEnumerable<Artifact> Artifacts { get; set; } = [];
        public int Total { get; set; }
    }
    public class ArtifactListResultWithMissingIds
    {
        public IEnumerable<Artifact> Artifacts { get; set; } = [];
        public bool IsAllFound { get; set; }
        public IEnumerable<Guid> MissingIds { get; set; } = [];
    }
    public class ArtifactRepository : IArtifactRepository
    {
        private readonly MuseTrip360DbContext _context;
        public ArtifactRepository(MuseTrip360DbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Artifact artifact)
        {
            await _context.Artifacts.AddAsync(artifact);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var artifact = await _context.Artifacts.FindAsync(id);
            if (artifact != null)
            {
                _context.Artifacts.Remove(artifact);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ArtifactList> GetAllAsync(ArtifactQuery query)
        {
            //get all artifacts with constraints and pagination
            var queryable = _context.Artifacts
                .Where(a => string.IsNullOrEmpty(query.SearchKeyword) || a.Name.Contains(query.SearchKeyword) || a.Description.Contains(query.SearchKeyword) || a.HistoricalPeriod.Contains(query.SearchKeyword))
                .Include(a => a.Events);
            //return the artifacts and the total
            var total = queryable.Count();
            var artifacts = await queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return new ArtifactList
            {
                Artifacts = artifacts,
                Total = total
            };
        }

        public async Task<ArtifactList> GetAllAdminAsync(ArtifactAdminQuery query)
        {
            //get all artifacts with constraints and pagination
            var queryable = _context.Artifacts
                .Where(a => query.IsActive == null || a.IsActive == query.IsActive)
                .Where(a => string.IsNullOrEmpty(query.SearchKeyword) || a.Name.Contains(query.SearchKeyword) || a.Description.Contains(query.SearchKeyword) || a.HistoricalPeriod.Contains(query.SearchKeyword));
            //return the artifacts and the total
            var total = queryable.Count();
            var artifacts = await queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return new ArtifactList
            {
                Artifacts = artifacts,
                Total = total
            };
        }

        public async Task<Artifact?> GetByIdAsync(Guid id)
        {
            return await _context.Artifacts.FindAsync(id);
        }

        public async Task UpdateAsync(Guid artifactId, Artifact artifact)
        {
            var existingArtifact = await GetByIdAsync(artifactId);
            if (existingArtifact != null)
            {
                _context.Entry(existingArtifact).CurrentValues.SetValues(artifact);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Artifact>> GetByMuseumIdAsync(Guid museumId)
        {
            return await _context.Artifacts.Where(a => a.MuseumId == museumId).ToListAsync();
        }

        public async Task<bool> IsMuseumExistsAsync(Guid museumId)
        {
            return await _context.Museums.AnyAsync(m => m.Id == museumId);
        }

        public async Task<bool> IsArtifactExistsAsync(Guid artifactId)
        {
            return await _context.Artifacts.AnyAsync(a => a.Id == artifactId);
        }

        public async Task<ArtifactListResultWithMissingIds> GetArtifactByListIdMuseumIdStatus(List<Guid> artifactIds, Guid museumId, bool status)
        {
            var foundArtifacts = await _context.Artifacts
                .Where(a => a.MuseumId == museumId)
                .Where(a => a.IsActive == status)
                .Where(a => artifactIds.Contains(a.Id))
                .ToListAsync();

            var foundIds = foundArtifacts.Select(a => a.Id).ToHashSet();
            var missingIds = new List<Guid>();
            foreach (var artifactId in artifactIds)
            {
                if (!foundIds.Contains(artifactId))
                {
                    missingIds.Add(artifactId);
                }
            }

            return new ArtifactListResultWithMissingIds
            {
                Artifacts = foundArtifacts,
                IsAllFound = missingIds.Count == 0,
                MissingIds = missingIds
            };
        }

        public async Task<ArtifactListResultWithMissingIds> GetArtifactByListIdEventId(List<Guid> artifactIds, Guid eventId)
        {
            var foundArtifacts = await _context.Artifacts
                .Where(a => a.Events.Any(e => e.Id == eventId))
                .Where(a => artifactIds.Contains(a.Id))
                .ToListAsync();

            var foundIds = foundArtifacts.Select(a => a.Id).ToHashSet();
            var missingIds = new List<Guid>();
            foreach (var artifactId in artifactIds)
            {
                if (!foundIds.Contains(artifactId))
                {
                    missingIds.Add(artifactId);
                }
            }

            return new ArtifactListResultWithMissingIds
            {
                Artifacts = foundArtifacts,
                IsAllFound = missingIds.Count == 0,
                MissingIds = missingIds
            };
        }
    }
}
