using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Dynamic.Core;
using Application.Shared.Enum;
using Database;
using Domain.Artifacts;
using Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using MuseTrip360.src.Application.DTOs.Artifact;

namespace MuseTrip360.src.Infrastructure.Repository
{
    public interface IArtifactRepository
    {
        Task<Artifact?> GetByIdAsync(Guid id);
        Task<ArtifactList> GetByMuseumIdAsync(Guid museumId, ArtifactAdminQuery query);
        Task<ArtifactList> GetAllAsync(ArtifactQuery query);
        Task<ArtifactList> GetAllAdminAsync(ArtifactAdminQuery query);
        Task AddAsync(Artifact artifact);
        Task UpdateAsync(Guid artifactId, Artifact artifact);
        Task DeleteAsync(Guid id);
        Task<bool> IsMuseumExistsAsync(Guid museumId);
        Task<bool> IsArtifactExistsAsync(Guid artifactId);
        Task<ArtifactListResultWithMissingIds> GetArtifactByListIdMuseumIdStatus(IEnumerable<Guid> artifactIds, Guid museumId, bool status);
        Task<ArtifactListResultWithMissingIds> GetArtifactByListIdEventId(IEnumerable<Guid> artifactIds, Guid eventId);
        Task<ArtifactList> GetArtifactByFilterSort(ArtifactFilterSort filterSort);
        Task FeedbackArtifacts(Guid artifactId, int rating, Guid userId, string comment);
        Task<IEnumerable<Feedback?>> GetFeedbackByArtifactIdAsync(Guid id);
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
                .Where(a => string.IsNullOrEmpty(query.Search) || a.Name.Contains(query.Search) || a.Description.Contains(query.Search) || a.HistoricalPeriod.Contains(query.Search))
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
                .Where(a => string.IsNullOrEmpty(query.Search) || a.Name.Contains(query.Search) || a.Description.Contains(query.Search) || a.HistoricalPeriod.Contains(query.Search));
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

        public async Task<ArtifactList> GetByMuseumIdAsync(Guid museumId, ArtifactAdminQuery query)
        {
            var queryable = _context.Artifacts
                .Where(a => a.MuseumId == museumId)
                .Where(a => query.IsActive == null || a.IsActive == query.IsActive)
                .Where(a => string.IsNullOrEmpty(query.Search) || a.Name.Contains(query.Search) || a.Description.Contains(query.Search) || a.HistoricalPeriod.Contains(query.Search));
            var total = queryable.Count();
            var artifacts = await queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return new ArtifactList
            {
                Artifacts = artifacts,
                Total = total
            };
        }

        public async Task<bool> IsMuseumExistsAsync(Guid museumId)
        {
            return await _context.Museums.AnyAsync(m => m.Id == museumId);
        }

        public async Task<bool> IsArtifactExistsAsync(Guid artifactId)
        {
            return await _context.Artifacts.AnyAsync(a => a.Id == artifactId);
        }

        public async Task<ArtifactListResultWithMissingIds> GetArtifactByListIdMuseumIdStatus(IEnumerable<Guid> artifactIds, Guid museumId, bool status)
        {
            var artifactIdsList = artifactIds.ToList(); // Convert to list once for multiple uses
            var foundArtifacts = await _context.Artifacts
                .Where(a => a.MuseumId == museumId)
                .Where(a => a.IsActive == status)
                .Where(a => artifactIdsList.Contains(a.Id))
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

        public async Task<ArtifactListResultWithMissingIds> GetArtifactByListIdEventId(IEnumerable<Guid> artifactIds, Guid eventId)
        {
            var artifactIdsList = artifactIds.ToList(); // Convert to list once for multiple uses
            var foundArtifacts = await _context.Artifacts
                .Where(a => a.Events.Any(e => e.Id == eventId))
                .Where(a => artifactIdsList.Contains(a.Id))
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

        public async Task<ArtifactList> GetArtifactByFilterSort(ArtifactFilterSort filterSort)
        {
            var direction = (filterSort.IsDescending == true ? "descending" : "ascending") ?? "descending";
            var sortFields = ArtifactFilterSort.GetNonNullFields(filterSort);
            var sortString = string.Join(", ", sortFields.Select(f => $"{f} {direction}"));

            var queryable = _context.Artifacts
                .Where(a =>
                    filterSort.Rating == null ||
                    (a.Rating >= filterSort.Rating.Value - 0.5f &&
                     a.Rating <= filterSort.Rating.Value + 0.5f))
                .Where(a => filterSort.IsActive == null || a.IsActive == filterSort.IsActive)
                .Where(a => filterSort.MuseumId == null || a.MuseumId == filterSort.MuseumId)
                .OrderBy(sortString);

            var total = await queryable.CountAsync();
            var artifacts = await queryable
                .Skip((filterSort.Page - 1) * filterSort.PageSize)
                .Take(filterSort.PageSize)
                .ToListAsync();

            return new ArtifactList
            {
                Artifacts = artifacts,
                Total = total
            };
        }

        public async Task FeedbackArtifacts(Guid artifactId, int rating, Guid userId, string comment)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var artifact = await _context.Artifacts.FindAsync(artifactId);
                if (artifact == null) throw new Exception("Artifact not found");

                // find feedback of user
                var userFeedback = await _context.Feedbacks
                    .FirstOrDefaultAsync(f => f.TargetId == artifactId && f.CreatedBy == userId);

                if (userFeedback != null)
                {
                    // update feedback
                    userFeedback.Rating = rating;
                    userFeedback.Comment = comment;
                }
                else
                {
                    // create new feedback
                    var newFeedback = new Feedback
                    {
                        TargetId = artifactId,
                        Type = DataEntityType.Artifact,
                        Rating = rating,
                        Comment = comment,
                        CreatedBy = userId
                    };
                    await _context.Feedbacks.AddAsync(newFeedback);
                }
                // save changes
                await _context.SaveChangesAsync();

                // calculate average rating
                var averageRating = await _context.Feedbacks
                    .Where(f => f.TargetId == artifactId)
                    .AverageAsync(f => f.Rating);

                // update artifact rating
                artifact.Rating = (float)averageRating;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("An error occurred while providing feedback for the tour online.", ex);
            }
            await transaction.CommitAsync();
        }

        public async Task<IEnumerable<Feedback?>> GetFeedbackByArtifactIdAsync(Guid id)
        {
            return await _context.Feedbacks
                .Include(f => f.CreatedByUser)
                .Where(f => f.TargetId == id)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
