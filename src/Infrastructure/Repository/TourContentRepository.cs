using Database;
using Domain.Tours;
using Microsoft.EntityFrameworkCore;
public interface ITourContentRepository
{
    Task<TourContent?> GetTourContentById(Guid id);
    Task<TourContentList> GetTourContents(TourContentQuery query);
    Task<TourContentList> GetTourContentsAdmin(TourContentAdminQuery query);
    Task<TourContentList> GetTourContentsByTourOnlineId(TourContentAdminQuery query, Guid tourOnlineId);
    Task CreateTourContent(TourContent tourContent);
    Task UpdateTourContent(Guid id, TourContent tourContent);
    Task DeleteTourContent(Guid id);
    Task<TourContentListWithMissingIds> GetTourContentsByListIdTourOnlineIdStatus(IEnumerable<Guid> tourContentIds, Guid tourOnlineId, bool isActive);
}

public class TourContentListWithMissingIds
{
    public IEnumerable<TourContent> Contents { get; set; } = [];
    public bool IsAllFound { get; set; }
    public IEnumerable<Guid> MissingIds { get; set; } = [];
}

public class TourContentList
{
    public IEnumerable<TourContent> Contents { get; set; } = [];
    public int Total { get; set; }
}
public class TourContentRepository : ITourContentRepository
{
    private readonly MuseTrip360DbContext _context;

    public TourContentRepository(MuseTrip360DbContext context)
    {
        _context = context;
    }

    public async Task CreateTourContent(TourContent tourContent)
    {
        await _context.TourContents.AddAsync(tourContent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTourContent(Guid id)
    {
        var tourContent = await _context.TourContents.FindAsync(id);
        if (tourContent != null)
        {
            _context.TourContents.Remove(tourContent);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<TourContent?> GetTourContentById(Guid id)
    {
        return await _context.TourContents.FindAsync(id);
    }

    public async Task<TourContentList> GetTourContents(TourContentQuery query)
    {
        var queryable = _context.TourContents
        .Where(x => query.TourId == null || x.TourId == query.TourId)
        .Where(x => query.Search == null || x.Content.Contains(query.Search));

        var total = await queryable.CountAsync();

        var contents = await queryable
        .OrderBy(x => x.ZOrder)
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();
        return new TourContentList { Contents = contents, Total = total };
    }

    public async Task<TourContentList> GetTourContentsAdmin(TourContentAdminQuery query)
    {
        var queryable = _context.TourContents
        .Where(x => query.IsActive == null || x.IsActive == query.IsActive)
        .Where(x => query.TourId == null || x.TourId == query.TourId)
        .Where(x => query.Search == null || x.Content.Contains(query.Search));

        var total = await queryable.CountAsync();

        var contents = await queryable
        .OrderBy(x => x.ZOrder)
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();
        return new TourContentList { Contents = contents, Total = total };

    }

    public async Task<TourContentListWithMissingIds> GetTourContentsByListIdTourOnlineIdStatus(IEnumerable<Guid> tourContentIds, Guid tourOnlineId, bool isActive)
    {
        var tourContentIdsList = tourContentIds.ToList();
        var queryable = _context.TourContents
        .Where(x => tourContentIdsList.Contains(x.Id))
        .Where(x => x.TourId == tourOnlineId)
        .Where(x => x.IsActive == isActive);

        var total = await queryable.CountAsync();
        var contents = await queryable.ToListAsync();
        return new TourContentListWithMissingIds { Contents = contents, IsAllFound = tourContentIdsList.Count() == total, MissingIds = tourContentIdsList.Except(contents.Select(x => x.Id)).ToList() };
    }

    public async Task<TourContentList> GetTourContentsByTourOnlineId(TourContentAdminQuery query, Guid tourOnlineId)
    {
        var queryable = _context.TourContents
        .Where(x => x.TourId == tourOnlineId)
        .Where(x => query.IsActive == null || x.IsActive == query.IsActive)
        .Where(x => query.TourId == null || x.TourId == query.TourId)
        .Where(x => query.Search == null || x.Content.Contains(query.Search));

        var total = await queryable.CountAsync();

        var contents = await queryable
        .OrderBy(x => x.ZOrder)
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();
        return new TourContentList { Contents = contents, Total = total };
    }

    public async Task UpdateTourContent(Guid id, TourContent tourContent)
    {
        var existingTourContent = await _context.TourContents.FindAsync(id);
        if (existingTourContent != null)
        {
            _context.Entry(existingTourContent).CurrentValues.SetValues(tourContent);
            await _context.SaveChangesAsync();
        }
    }
}

