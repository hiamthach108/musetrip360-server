using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Shared.Enum;
using Database;
using Domain.Events;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repository
{
    public interface IEventRepository
    {
        Task<Event?> GetEventById(Guid id);
        Task<EventList> GetAllAsync(EventQuery query);
        Task<EventList> GetAllAdminAsync(EventAdminQuery query);
        Task AddAsync(Event eventItem);
        Task UpdateAsync(Guid id, Event eventItem);
        Task DeleteAsync(Guid id);
        Task<bool> IsEventExistsAsync(Guid id);
        Task<IEnumerable<Event>> GetEventsByMuseumIdAsync(Guid museumId);
        Task<IEnumerable<Event>> GetAllEventByOrganizerAsync(Guid userId, EventStatusEnum? status);
        Task<bool> IsOwner(Guid userId, Guid eventId);
    }

    public class EventList
    {
        public IEnumerable<Event> Events { get; set; } = [];
        public int Total { get; set; }
    }

    public class EventRepository : IEventRepository
    {
        private readonly MuseTrip360DbContext _context;
        public EventRepository(MuseTrip360DbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Event eventItem)
        {
            await _context.Events.AddAsync(eventItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var eventItem = await _context.Events.FindAsync(id);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EventList> GetAllAdminAsync(EventAdminQuery query)
        {
            // fetch all events that match the query
            var queryable = _context.Events
            .Where(e => query.MuseumId == null || e.MuseumId == query.MuseumId)
            .Where(e => string.IsNullOrEmpty(query.Search) || e.Title.Contains(query.Search) || e.Description.Contains(query.Search))
            .Where(e => string.IsNullOrEmpty(query.Location) || e.Location.Contains(query.Location))
            .Where(e => query.EventType == null || e.EventType == query.EventType)
            .Where(e => query.Status == null || e.Status == query.Status)
            // time range available is e.Start before query end or query Start before e.End is available
            .Where(e => query.StartTime == null || e.StartTime <= query.EndTime)
            .Where(e => query.EndTime == null || e.EndTime >= query.StartTime)
            .Where(e => query.StartBookingDeadline == null || e.BookingDeadline >= query.StartBookingDeadline)
            .Where(e => query.EndBookingDeadline == null || e.BookingDeadline <= query.EndBookingDeadline)
            .Include(e => e.Artifacts)
            .Include(e => e.TourOnlines)
            .Include(e => e.TourGuides)
            .Include(e => e.TicketAddons);

            var total = queryable.Count();
            var events = await queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return new EventList
            {
                Events = events,
                Total = total
            };
        }

        public async Task<EventList> GetAllAsync(EventQuery query)
        {
            // fetch all events that match the query
            var queryable = _context.Events
            .Where(e => query.MuseumId == null || e.MuseumId == query.MuseumId)
            .Where(e => string.IsNullOrEmpty(query.Search) || e.Title.Contains(query.Search) || e.Description.Contains(query.Search))
            .Where(e => string.IsNullOrEmpty(query.Location) || e.Location.Contains(query.Location))
            .Where(e => query.EventType == null || e.EventType == query.EventType)
            // time range available is e.Start before query end or query Start before e.End is available
            .Where(e => query.StartTime == null || e.StartTime <= query.EndTime)
            .Where(e => query.EndTime == null || e.EndTime >= query.StartTime)
            .Include(e => e.Artifacts)
            .Include(e => e.TourOnlines)
            .Include(e => e.TourGuides)
            .Include(e => e.TicketAddons);

            var total = queryable.Count();
            var events = await queryable.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync();

            return new EventList
            {
                Events = events,
                Total = total
            };
        }

        public async Task<Event?> GetEventById(Guid id)
        {
            return await _context.Events
            .Include(e => e.Artifacts)
            .Include(e => e.TourOnlines)
            .Include(e => e.TourGuides)
            .Include(e => e.TicketAddons)
            .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> IsEventExistsAsync(Guid id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }

        public async Task UpdateAsync(Guid id, Event eventItem)
        {
            var existingEvent = await GetEventById(id);
            if (existingEvent != null)
            {
                _context.Entry(existingEvent).CurrentValues.SetValues(eventItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Event>> GetEventsByMuseumIdAsync(Guid museumId)
        {
            return await _context.Events.Where(e => e.MuseumId == museumId)
            .Include(e => e.Artifacts)
            .Include(e => e.TourOnlines)
            .Include(e => e.TourGuides)
            .Include(e => e.TicketAddons)
            .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetAllEventByOrganizerAsync(Guid userId, EventStatusEnum? status)
        {
            return await _context.Events
            .Where(e => e.CreatedBy == userId && (status == null || e.Status == status))
            .Include(e => e.Artifacts)
            .Include(e => e.TourOnlines)
            .Include(e => e.TourGuides)
            .Include(e => e.TicketAddons)
            .ToListAsync();
        }

        public async Task<bool> IsOwner(Guid userId, Guid eventId)
        {
            return await _context.Events.AnyAsync(e => e.CreatedBy == userId && e.Id == eventId);
        }
    }
}