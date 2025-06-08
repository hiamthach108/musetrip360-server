using Database;
using Domain.Tickets;
using Microsoft.EntityFrameworkCore;

public interface ITicketMasterRepository
{
    Task<TicketMaster?> GetTicketMasterById(Guid id);
    Task<IEnumerable<TicketMaster>> GetTicketMasterByMuseumId(Guid museumId);
    Task CreateTicketMaster(TicketMaster ticketMaster);
    Task UpdateTicketMaster(Guid id, TicketMaster ticketMaster);
    Task DeleteTicketMaster(Guid id);
    Task<TicketMasterList> GetTicketMasterQuery(TicketMasterQuery query);
}

public class TicketMasterList
{
    public IEnumerable<TicketMaster> TicketMasters { get; set; } = [];
    public int Total { get; set; }
}

public class TicketMasterRepository : ITicketMasterRepository
{
    private readonly MuseTrip360DbContext _context;
    public TicketMasterRepository(MuseTrip360DbContext context)
    {
        _context = context;
    }

    public async Task CreateTicketMaster(TicketMaster ticketMaster)
    {
        _context.TicketMasters.Add(ticketMaster);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTicketMaster(Guid id)
    {
        var ticketMaster = await _context.TicketMasters.FindAsync(id);
        if (ticketMaster != null)
        {
            _context.TicketMasters.Remove(ticketMaster);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<TicketMaster>> GetTicketMasterByMuseumId(Guid museumId)
    {
        var ticketMasters = await _context.TicketMasters.Where(x => x.MuseumId == museumId)
        .Include(x => x.Tickets)
        .Include(x => x.OrderTickets)
        .ToArrayAsync();
        return ticketMasters;
    }

    public async Task<TicketMaster?> GetTicketMasterById(Guid id)
    {
        var ticketMaster = await _context.TicketMasters
        .Include(x => x.Tickets)
        .Include(x => x.OrderTickets)
        .FirstOrDefaultAsync(x => x.Id == id);
        return ticketMaster;
    }

    public async Task UpdateTicketMaster(Guid id, TicketMaster ticketMaster)
    {
        var ticketMasterToUpdate = await _context.TicketMasters.FindAsync(id);
        if (ticketMasterToUpdate != null)
        {
            _context.TicketMasters.Entry(ticketMasterToUpdate).CurrentValues.SetValues(ticketMaster);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<TicketMasterList> GetTicketMasterQuery(TicketMasterQuery query)
    {
        var queryable = _context.TicketMasters
        .Where(x => !query.MuseumId.HasValue || x.MuseumId == query.MuseumId)
        .Where(x => string.IsNullOrEmpty(query.SearchKeyword) || x.Name.Contains(query.SearchKeyword) || x.Description.Contains(query.SearchKeyword))
        .Where(x => !query.Price.HasValue || x.Price == query.Price)
        .Where(x => !query.DiscountPercentage.HasValue || x.DiscountPercentage == query.DiscountPercentage)
        .Where(x => !query.GroupSize.HasValue || x.GroupSize == query.GroupSize)
        .Include(x => x.Tickets)
        .Include(x => x.OrderTickets);

        var total = await queryable.CountAsync();
        var ticketMasters = await queryable
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize).ToArrayAsync();
        return new TicketMasterList { TicketMasters = ticketMasters, Total = total };
    }
}