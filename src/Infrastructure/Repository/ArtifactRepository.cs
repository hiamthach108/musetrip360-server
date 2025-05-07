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
        Task DeleteAsync(Artifact artifact);
    }
    public class ArtifactList
    {
        public IEnumerable<Artifact> Artifacts { get; set; } = [];
        public int Total { get; set; }
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

        public async Task DeleteAsync(Artifact artifact)
        {
            _context.Remove(artifact);
            await _context.SaveChangesAsync();
        }

        public async Task<ArtifactList> GetAllAsync(ArtifactQuery query)
        {
            //count all artifacts with constraints
            var total = await _context.Artifacts
                .Where(a => string.IsNullOrEmpty(query.SearchKeyword) || a.Name.Contains(query.SearchKeyword) || a.Description.Contains(query.SearchKeyword) || a.HistoricalPeriod.Contains(query.SearchKeyword))
                .CountAsync();
            //get all artifacts with constraints and pagination
            var queryable = await _context.Artifacts
                .Where(a => string.IsNullOrEmpty(query.SearchKeyword) || a.Name.Contains(query.SearchKeyword) || a.Description.Contains(query.SearchKeyword) || a.HistoricalPeriod.Contains(query.SearchKeyword))
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
            //return the artifacts and the total
            return new ArtifactList
            {
                Artifacts = queryable,
                Total = total
            };
        }

        public async Task<ArtifactList> GetAllAdminAsync(ArtifactAdminQuery query)
        {
            //count all artifacts with constraints
            var total = await _context.Artifacts
                .Where(a => query.IsActive == null || a.IsActive == query.IsActive)
                .Where(a => string.IsNullOrEmpty(query.SearchKeyword) || a.Name.Contains(query.SearchKeyword) || a.Description.Contains(query.SearchKeyword) || a.HistoricalPeriod.Contains(query.SearchKeyword))
                .CountAsync();
            //get all artifacts with constraints and pagination
            var queryable = await _context.Artifacts
                .Where(a => query.IsActive == null || a.IsActive == query.IsActive)
                .Where(a => string.IsNullOrEmpty(query.SearchKeyword) || a.Name.Contains(query.SearchKeyword) || a.Description.Contains(query.SearchKeyword) || a.HistoricalPeriod.Contains(query.SearchKeyword))
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
            //return the artifacts and the total
            return new ArtifactList
            {
                Artifacts = queryable,
                Total = total
            };
        }

        public async Task<Artifact?> GetByIdAsync(Guid id)
        {
            return await _context.Artifacts.FindAsync(id);
        }

        public async Task UpdateAsync(Guid artifactId, Artifact artifact)
        {
            var existingArtifact = GetByIdAsync(artifactId);
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
    }
}
