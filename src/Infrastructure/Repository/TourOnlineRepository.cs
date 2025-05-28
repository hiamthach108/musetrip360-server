using Database;
using Domain.Tours;
using Microsoft.EntityFrameworkCore;

public interface ITourOnlineRepository
{
    Task<TourOnline?> GetByIdAsync(Guid id);
    Task<TourOnlineList> GetAllAsync(TourOnlineQuery query);
    Task<TourOnlineList> GetAllAdminAsync(TourOnlineAdminQuery query);
    Task<IEnumerable<TourOnline>> GetAllByMuseumIdAsync(Guid museumId);
    Task CreateAsync(TourOnline tourOnline);
    Task UpdateAsync(TourOnline tourOnline);
    Task DeleteAsync(Guid id);
    Task<bool> IsTourOnlineExists(Guid id);
    Task<TourOnlineListResultWithMissingIds> GetTourOnlineByListIdMuseumIdStatus(IEnumerable<Guid> tourOnlineIds, Guid museumId, bool IsActive);
    Task<TourOnlineListResultWithMissingIds> GetTourOnlineByListIdEventId(IEnumerable<Guid> tourOnlineIds, Guid eventId);
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

    public async Task<IEnumerable<TourOnline>> GetAllByMuseumIdAsync(Guid museumId)
    {
        return await _context.TourOnlines
        .Where(t => t.MuseumId == museumId)
        .Include(t => t.TourContents)
        .ToListAsync();
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
        var tourOnlineIdsList = tourOnlineIds.ToList(); // Convert to list once for multiple uses
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
        var tourOnlineIdsList = tourOnlineIds.ToList(); // Convert to list once for multiple uses
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
}
