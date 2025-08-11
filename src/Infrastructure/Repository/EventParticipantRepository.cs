namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Domain.Events;
using Microsoft.EntityFrameworkCore;

public interface IEventParticipantRepository
{
  Task<EventParticipant> GetByIdAsync(Guid id);
  Task<IEnumerable<EventParticipant>> GetAllAsync();
  Task<IEnumerable<EventParticipant>> GetByEventIdAsync(Guid eventId);
  Task<IEnumerable<EventParticipant>> GetByUserIdAsync(Guid userId);
  Task<EventParticipant> GetByEventIdAndUserIdAsync(Guid eventId, Guid userId);
  Task<EventParticipant> AddAsync(EventParticipant eventParticipant);
  Task<EventParticipant> UpdateAsync(Guid eventParticipantId, EventParticipant eventParticipant);
  Task<EventParticipant> DeleteAsync(EventParticipant eventParticipant);
  Task<bool> ValidateUser(Guid userId, Guid eventId);
  Task AddRangeAsync(IEnumerable<EventParticipant> eventParticipants);
}

public class EventParticipantRepository : IEventParticipantRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public EventParticipantRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<EventParticipant> GetByIdAsync(Guid id)
  {
    var eventParticipant = await _dbContext.EventParticipants.FindAsync(id);
    return eventParticipant;
  }

  public async Task<IEnumerable<EventParticipant>> GetAllAsync()
  {
    var eventParticipants = _dbContext.EventParticipants.AsEnumerable();
    return eventParticipants;
  }

  public async Task<IEnumerable<EventParticipant>> GetByEventIdAsync(Guid eventId)
  {
    var eventParticipants = _dbContext.EventParticipants
      .Where(ep => ep.EventId == eventId)
      .OrderByDescending(ep => ep.JoinedAt)
      .AsEnumerable();
    return eventParticipants;
  }

  public async Task<IEnumerable<EventParticipant>> GetByUserIdAsync(Guid userId)
  {
    var eventParticipants = _dbContext.EventParticipants
      .Where(ep => ep.UserId == userId)
      .OrderByDescending(ep => ep.JoinedAt)
      .AsEnumerable();
    return eventParticipants;
  }

  public async Task<EventParticipant> GetByEventIdAndUserIdAsync(Guid eventId, Guid userId)
  {
    var eventParticipant = _dbContext.EventParticipants
      .FirstOrDefault(ep => ep.EventId == eventId && ep.UserId == userId);
    return eventParticipant;
  }

  public async Task<EventParticipant> AddAsync(EventParticipant eventParticipant)
  {
    var result = await _dbContext.EventParticipants.AddAsync(eventParticipant);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<EventParticipant> UpdateAsync(Guid eventParticipantId, EventParticipant eventParticipant)
  {
    var existingEventParticipant = await _dbContext.EventParticipants.FindAsync(eventParticipantId);
    if (existingEventParticipant == null) return null;

    _dbContext.Entry(existingEventParticipant).CurrentValues.SetValues(eventParticipant);
    await _dbContext.SaveChangesAsync();
    return existingEventParticipant;
  }

  public async Task<EventParticipant> DeleteAsync(EventParticipant eventParticipant)
  {
    var result = _dbContext.EventParticipants.Remove(eventParticipant);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<bool> ValidateUser(Guid userId, Guid eventId)
  {
    var eventParticipant = await _dbContext.EventParticipants.FirstOrDefaultAsync(ep => ep.UserId == userId && ep.EventId == eventId);
    return eventParticipant != null;
  }

  public async Task AddRangeAsync(IEnumerable<EventParticipant> eventParticipants)
  {
    await _dbContext.EventParticipants.AddRangeAsync(eventParticipants);
    await _dbContext.SaveChangesAsync();
  }
}