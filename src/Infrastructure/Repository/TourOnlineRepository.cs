using Application.Shared.Enum;
using Database;
using Domain.Reviews;
using Domain.Tours;
using Microsoft.EntityFrameworkCore;

public interface ITourOnlineRepository
{
    Task<TourOnline?> GetByIdAsync(Guid id);
    Task<TourOnlineList> GetAllAsync(TourOnlineQuery query);
    Task<TourOnlineList> GetAllAdminAsync(TourOnlineAdminQuery query);
    Task<TourOnlineList> GetAllByMuseumIdAsync(Guid museumId, TourOnlineAdminQuery query);
    Task CreateAsync(TourOnline tourOnline);
    Task UpdateAsync(TourOnline tourOnline);
    Task DeleteAsync(Guid id);
    Task<bool> IsTourOnlineExists(Guid id);
    Task<TourOnlineListResultWithMissingIds> GetTourOnlineByListIdMuseumIdStatus(IEnumerable<Guid> tourOnlineIds, Guid museumId, bool IsActive);
    Task<TourOnlineListResultWithMissingIds> GetTourOnlineByListIdEventId(IEnumerable<Guid> tourOnlineIds, Guid eventId);
    Task FeedbackTourOnlines(Guid tourOnlineId, Guid userId, string comment);
}
public class TourOnlineList
{
    public IEnumerable<TourOnline> Tours { get; set; } = [];
    public int Total { get; set; }
}
public class TourOnlineListResultWithMissingIds
{
    public IEnumerable<TourOnline> Tours { get; set; } = [];
    public bool IsAllFound { get; set; }
    public List<Guid> MissingIds { get; set; } = [];
}
public class TourOnlineRepository : ITourOnlineRepository
{
    private readonly MuseTrip360DbContext _context;
    public TourOnlineRepository(MuseTrip360DbContext context)
    {
        _context = context;
    }
    public async Task CreateAsync(TourOnline tourOnline)
    {
        await _context.TourOnlines.AddAsync(tourOnline);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tourOnline = await _context.TourOnlines.FindAsync(id);
        if (tourOnline != null)
        {
            tourOnline.IsActive = false;
            await UpdateAsync(tourOnline);
        }
    }

    public async Task<TourOnlineList> GetAllAsync(TourOnlineQuery query)
    {
        var queryable = _context.TourOnlines
        .Where(t => t.IsActive == true)
        .Where(t => query.MuseumId == null || t.MuseumId == query.MuseumId)
        .Where(t => query.SearchKeyword == null || t.Name.Contains(query.SearchKeyword) || t.Description.Contains(query.SearchKeyword))
        .Include(t => t.TourContents);
        var total = await queryable.CountAsync();

        var tours = await queryable
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new TourOnlineList
        {
            Tours = tours,
            Total = total
        };
    }

    public async Task<TourOnlineList> GetAllAdminAsync(TourOnlineAdminQuery query)
    {
        var queryable = _context.TourOnlines
        .Where(t => query.IsActive == null || t.IsActive == query.IsActive)
        .Where(t => query.MuseumId == null || t.MuseumId == query.MuseumId)
        .Where(t => query.SearchKeyword == null || t.Name.Contains(query.SearchKeyword) || t.Description.Contains(query.SearchKeyword))
        .Include(t => t.TourContents);
        var total = await queryable.CountAsync();

        var tours = await queryable
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new TourOnlineList { Tours = tours, Total = total };
    }

    public async Task<TourOnlineList> GetAllByMuseumIdAsync(Guid museumId, TourOnlineAdminQuery query)
    {
        var queryable = _context.TourOnlines
        .Where(t => query.IsActive == null || t.IsActive == query.IsActive)
        .Where(t => t.MuseumId == museumId)
        .Where(t => query.SearchKeyword == null || t.Name.Contains(query.SearchKeyword) || t.Description.Contains(query.SearchKeyword))
        .Include(t => t.TourContents);
        var total = await queryable.CountAsync();

        var tours = await queryable
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new TourOnlineList { Tours = tours, Total = total };
    }

    public async Task<TourOnline?> GetByIdAsync(Guid id)
    {
        return await _context.TourOnlines
        .Where(t => t.Id == id)
        .Include(t => t.TourContents)
        .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(TourOnline tourOnline)
    {
        var existTourOnline = await GetByIdAsync(tourOnline.Id);
        if (existTourOnline != null)
        {
            _context.Entry(existTourOnline).CurrentValues.SetValues(tourOnline);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsTourOnlineExists(Guid id)
    {
        return await _context.TourOnlines.AnyAsync(t => t.Id == id);
    }

    public async Task<TourOnlineListResultWithMissingIds> GetTourOnlineByListIdMuseumIdStatus(IEnumerable<Guid> tourOnlineIds, Guid museumId, bool isActive)
    {
        var tourOnlineIdsList = tourOnlineIds.ToList();
        var queryable = _context.TourOnlines
        .Where(t => tourOnlineIdsList.Contains(t.Id))
        .Where(t => t.MuseumId == museumId)
        .Where(t => t.IsActive == isActive);

        var total = await queryable.CountAsync();
        var tours = await queryable.ToListAsync();

        return new TourOnlineListResultWithMissingIds
        {
            Tours = tours,
            IsAllFound = tourOnlineIdsList.Count == total,
            MissingIds = tourOnlineIdsList.Except(tours.Select(t => t.Id)).ToList()
        };
    }

    public async Task<TourOnlineListResultWithMissingIds> GetTourOnlineByListIdEventId(IEnumerable<Guid> tourOnlineIds, Guid eventId)
    {
        var tourOnlineIdsList = tourOnlineIds.ToList();
        var queryable = _context.TourOnlines
        .Where(t => tourOnlineIdsList.Contains(t.Id))
        .Where(t => t.Events.Any(e => e.Id == eventId));

        var total = await queryable.CountAsync();
        var tours = await queryable.ToListAsync();

        return new TourOnlineListResultWithMissingIds
        {
            Tours = tours,
            IsAllFound = tourOnlineIdsList.Count == total,
            MissingIds = tourOnlineIdsList.Except(tours.Select(t => t.Id)).ToList()
        };
    }

    public async Task FeedbackTourOnlines(Guid tourOnlineId, Guid userId, string comment)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var tourOnline = await _context.TourOnlines.FindAsync(tourOnlineId);
            if (tourOnline == null) throw new Exception("Tour online not found");

            // find feedback of user
            var userFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.TargetId == tourOnlineId && f.CreatedBy == userId);

            if (userFeedback != null)
            {
                // update feedback
                userFeedback.Comment = comment;
            }
            else
            {
                // create new feedback
                var newFeedback = new Feedback
                {
                    TargetId = tourOnlineId,
                    Type = DataEntityType.TourOnline,
                    Rating = 0,
                    Comment = comment,
                    CreatedBy = userId
                };
                await _context.Feedbacks.AddAsync(newFeedback);
            }
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("An error occurred while providing feedback for the tour online.", ex);
        }
        await transaction.CommitAsync();
    }
}
