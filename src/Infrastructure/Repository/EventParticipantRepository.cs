namespace Infrastructure.Repository;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using Domain.Events;
using Microsoft.EntityFrameworkCore;

public interface IEventParticipantRepository
{
  Task<EventParticipant?> GetByIdAsync(Guid id);
  Task<List<EventParticipant>> GetAllAsync();
  Task<List<EventParticipant>> GetByEventIdAsync(Guid eventId);
  Task<List<EventParticipant>> GetByUserIdAsync(Guid userId);
  Task<EventParticipant?> GetByEventIdAndUserIdAsync(Guid eventId, Guid userId);
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

  public async Task<EventParticipant?> GetByIdAsync(Guid id)
  {
    var eventParticipants = await _dbContext.EventParticipants
      .Include(ep => ep.User)
      .Select(ep => new EventParticipant
      {
        Id = ep.Id,
        EventId = ep.EventId,
        UserId = ep.UserId,
        JoinedAt = ep.JoinedAt,
        Role = ep.Role,
        Status = ep.Status,
        Event = new Domain.Events.Event
        {
          Id = ep.Event.Id,
          Title = ep.Event.Title,
          Description = ep.Event.Description,
          StartTime = ep.Event.StartTime,
          EndTime = ep.Event.EndTime,
          Location = ep.Event.Location,
          Capacity = ep.Event.Capacity,
          AvailableSlots = ep.Event.AvailableSlots,
          BookingDeadline = ep.Event.BookingDeadline,
          MuseumId = ep.Event.MuseumId,
          CreatedBy = ep.Event.CreatedBy,
          Status = ep.Event.Status,
          Price = ep.Event.Price
        },
        User = new Domain.Users.User
        {
          Id = ep.User.Id,
          Username = ep.User.Username,
          FullName = ep.User.FullName,
          Email = ep.User.Email,
          PhoneNumber = ep.User.PhoneNumber,
          AvatarUrl = ep.User.AvatarUrl,
          BirthDate = ep.User.BirthDate,
          AuthType = ep.User.AuthType,
          Status = ep.User.Status,
          LastLogin = ep.User.LastLogin,
          CreatedAt = ep.User.CreatedAt,
          UpdatedAt = ep.User.UpdatedAt,
        }
      })
      .ToListAsync();
    return eventParticipants.FirstOrDefault(ep => ep.Id == id);
  }

  public async Task<List<EventParticipant>> GetAllAsync()
  {
    var eventParticipants = await _dbContext.EventParticipants
      .Include(ep => ep.User)
      .Select(ep => new EventParticipant
      {
        Id = ep.Id,
        EventId = ep.EventId,
        UserId = ep.UserId,
        JoinedAt = ep.JoinedAt,
        Role = ep.Role,
        Status = ep.Status,
        Event = new Domain.Events.Event
        {
          Id = ep.Event.Id,
          Title = ep.Event.Title,
          Description = ep.Event.Description,
          StartTime = ep.Event.StartTime,
          EndTime = ep.Event.EndTime,
          Location = ep.Event.Location,
          Capacity = ep.Event.Capacity,
          AvailableSlots = ep.Event.AvailableSlots,
          BookingDeadline = ep.Event.BookingDeadline,
          MuseumId = ep.Event.MuseumId,
          CreatedBy = ep.Event.CreatedBy,
          Status = ep.Event.Status,
          Price = ep.Event.Price
        },
        User = new Domain.Users.User
        {
          Id = ep.User.Id,
          Username = ep.User.Username,
          FullName = ep.User.FullName,
          Email = ep.User.Email,
          PhoneNumber = ep.User.PhoneNumber,
          AvatarUrl = ep.User.AvatarUrl,
          BirthDate = ep.User.BirthDate,
          AuthType = ep.User.AuthType,
          Status = ep.User.Status,
          LastLogin = ep.User.LastLogin,
          CreatedAt = ep.User.CreatedAt,
          UpdatedAt = ep.User.UpdatedAt,
        }
      })
      .ToListAsync();
    return eventParticipants;
  }

  public async Task<List<EventParticipant>> GetByEventIdAsync(Guid eventId)
  {
    var eventParticipants = await _dbContext.EventParticipants
      .Include(ep => ep.User)
      .Select(ep => new EventParticipant
      {
        Id = ep.Id,
        EventId = ep.EventId,
        UserId = ep.UserId,
        JoinedAt = ep.JoinedAt,
        Role = ep.Role,
        Status = ep.Status,
        Event = new Domain.Events.Event
        {
          Id = ep.Event.Id,
          Title = ep.Event.Title,
          Description = ep.Event.Description,
          StartTime = ep.Event.StartTime,
          EndTime = ep.Event.EndTime,
          Location = ep.Event.Location,
          Capacity = ep.Event.Capacity,
          AvailableSlots = ep.Event.AvailableSlots,
          BookingDeadline = ep.Event.BookingDeadline,
          MuseumId = ep.Event.MuseumId,
          CreatedBy = ep.Event.CreatedBy,
          Status = ep.Event.Status,
          Price = ep.Event.Price
        },
        User = new Domain.Users.User
        {
          Id = ep.User.Id,
          Username = ep.User.Username,
          FullName = ep.User.FullName,
          Email = ep.User.Email,
          PhoneNumber = ep.User.PhoneNumber,
          AvatarUrl = ep.User.AvatarUrl,
          BirthDate = ep.User.BirthDate,
          AuthType = ep.User.AuthType,
          Status = ep.User.Status,
          LastLogin = ep.User.LastLogin,
          CreatedAt = ep.User.CreatedAt,
          UpdatedAt = ep.User.UpdatedAt,
        }
      })
      .Where(ep => ep.EventId == eventId)
      .OrderByDescending(ep => ep.JoinedAt)
      .ToListAsync();
    return eventParticipants;
  }

  public async Task<List<EventParticipant>> GetByUserIdAsync(Guid userId)
  {
    var eventParticipants = await _dbContext.EventParticipants
      .Include(ep => ep.User)
      .Select(ep => new EventParticipant
      {
        Id = ep.Id,
        EventId = ep.EventId,
        UserId = ep.UserId,
        JoinedAt = ep.JoinedAt,
        Role = ep.Role,
        Status = ep.Status,
        Event = new Domain.Events.Event
        {
          Id = ep.Event.Id,
          Title = ep.Event.Title,
          Description = ep.Event.Description,
          StartTime = ep.Event.StartTime,
          EndTime = ep.Event.EndTime,
          Location = ep.Event.Location,
          Capacity = ep.Event.Capacity,
          AvailableSlots = ep.Event.AvailableSlots,
          BookingDeadline = ep.Event.BookingDeadline,
          MuseumId = ep.Event.MuseumId,
          CreatedBy = ep.Event.CreatedBy,
          Status = ep.Event.Status,
          Price = ep.Event.Price
        },
        User = new Domain.Users.User
        {
          Id = ep.User.Id,
          Username = ep.User.Username,
          FullName = ep.User.FullName,
          Email = ep.User.Email,
          PhoneNumber = ep.User.PhoneNumber,
          AvatarUrl = ep.User.AvatarUrl,
          BirthDate = ep.User.BirthDate,
          AuthType = ep.User.AuthType,
          Status = ep.User.Status,
          LastLogin = ep.User.LastLogin,
          CreatedAt = ep.User.CreatedAt,
          UpdatedAt = ep.User.UpdatedAt,
        }
      })
      .Where(ep => ep.UserId == userId)
      .OrderByDescending(ep => ep.CreatedAt)
      .ToListAsync();
    return eventParticipants;
  }

  public async Task<EventParticipant?> GetByEventIdAndUserIdAsync(Guid eventId, Guid userId)
  {
    var eventParticipants = await _dbContext.EventParticipants
      .Include(ep => ep.User)
      .Select(ep => new EventParticipant
      {
        Id = ep.Id,
        EventId = ep.EventId,
        UserId = ep.UserId,
        JoinedAt = ep.JoinedAt,
        Role = ep.Role,
        Status = ep.Status,
        Event = new Domain.Events.Event
        {
          Id = ep.Event.Id,
          Title = ep.Event.Title,
          Description = ep.Event.Description,
          StartTime = ep.Event.StartTime,
          EndTime = ep.Event.EndTime,
          Location = ep.Event.Location,
          Capacity = ep.Event.Capacity,
          AvailableSlots = ep.Event.AvailableSlots,
          BookingDeadline = ep.Event.BookingDeadline,
          MuseumId = ep.Event.MuseumId,
          CreatedBy = ep.Event.CreatedBy,
          Status = ep.Event.Status,
          Price = ep.Event.Price
        },
        User = new Domain.Users.User
        {
          Id = ep.User.Id,
          Username = ep.User.Username,
          FullName = ep.User.FullName,
          Email = ep.User.Email,
          PhoneNumber = ep.User.PhoneNumber,
          AvatarUrl = ep.User.AvatarUrl,
          BirthDate = ep.User.BirthDate,
          AuthType = ep.User.AuthType,
          Status = ep.User.Status,
          LastLogin = ep.User.LastLogin,
          CreatedAt = ep.User.CreatedAt,
          UpdatedAt = ep.User.UpdatedAt,
        }
      })
      .Where(ep => ep.UserId == userId && ep.EventId == eventId)
      .OrderByDescending(ep => ep.CreatedAt)
      .ToListAsync();
    return eventParticipants.FirstOrDefault(ep => ep.UserId == userId && ep.EventId == eventId);
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