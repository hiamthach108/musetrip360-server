using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Events;
namespace Infrastructure.Repository
{
    public interface IEventRepository
    {
        Task<Event?> GetEventById(int id);
        Task<IEnumerable<Event>> GetAllAsync();
        Task<EventList> GetAllAdminAsync(EventAdminQuery query);
        Task<Event> AddAsync(Event eventItem);
        Task<Event> UpdateAsync(Event eventItem);
        Task<bool> DeleteAsync(int id);
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
}