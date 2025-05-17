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
}
public class TourOnlineList
{
    public IEnumerable<TourOnline> Tours { get; set; } = [];
    public int Total { get; set; }
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
        if (tourOnline == null)
        {
            throw new Exception("Tour online not found");
        }
        _context.TourOnlines.Remove(tourOnline);
        await _context.SaveChangesAsync();
    }

    public async Task<TourOnlineList> GetAllAsync(TourOnlineQuery query)
    {
        var queryable = _context.TourOnlines
        .Where(t => t.IsActive == true)
        .Where(t => query.MuseumId == null || t.MuseumId == query.MuseumId)
        .Where(t => query.SearchKeyword == null || t.Name.Contains(query.SearchKeyword) || t.Description.Contains(query.SearchKeyword));
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
        .Where(t => query.SearchKeyword == null || t.Name.Contains(query.SearchKeyword) || t.Description.Contains(query.SearchKeyword));
        var total = await queryable.CountAsync();

        var tours = await queryable
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

        return new TourOnlineList { Tours = tours, Total = total };
    }

    public async Task<IEnumerable<TourOnline>> GetAllByMuseumIdAsync(Guid museumId)
    {
        return await _context.TourOnlines.Where(t => t.MuseumId == museumId).ToListAsync();
    }

    public async Task<TourOnline?> GetByIdAsync(Guid id)
    {
        return await _context.TourOnlines.FindAsync(id);
    }

    public async Task UpdateAsync(TourOnline tourOnline)
    {
        var existTourOnline = await GetByIdAsync(tourOnline.Id);
        if (existTourOnline == null)
        {
            throw new Exception("Tour online not found");
        }
        _context.Entry(existTourOnline).CurrentValues.SetValues(tourOnline);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsTourOnlineExists(Guid id)
    {
        return await _context.TourOnlines.AnyAsync(t => t.Id == id);
    }
}
