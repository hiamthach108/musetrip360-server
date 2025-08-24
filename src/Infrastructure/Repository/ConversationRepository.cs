namespace Infrastructure.Repository;

using System;
using Application.DTOs.Chat;
using Database;
using Domain.Messaging;
using Microsoft.EntityFrameworkCore;

public interface IConversationRepository
{
  // Conversation
  Task<Conversation?> GetByIdAsync(Guid id);
  IEnumerable<Conversation> GetConversationUsers(Guid userId);
  ConversationUser? GetConversationUser(Guid conversationId, Guid userId);
  Task<Conversation> CreateConversation(Conversation conversation);
  Task<IEnumerable<ConversationUser>> AddUsersToConversation(Guid conversationId, List<Guid> userIds);
  Task<int> UpdateLastSeen(Guid conversationId, Guid userId);
  Task<int> UpdateName(Guid conversationId, string name);
  Task<List<Guid>> GetConversationUserIds(Guid conversationId);
  Task<Conversation> GetConversationByUsers(Guid userId1, Guid userId2);
  Task<Conversation?> UpdateConversation(Guid conversationId, UpdateConversation updateData);
  Task<bool> DeleteConversation(Guid conversationId);
}

public class ConversationList
{
  public IEnumerable<Conversation> Conversations { get; set; } = [];
  public int Total { get; set; }
}

public class ConversationRepository : IConversationRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public ConversationRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<IEnumerable<ConversationUser>> AddUsersToConversation(Guid conversationId, List<Guid> userIds)
  {
    var listConversationUser = new List<ConversationUser>();
    foreach (var userId in userIds)
    {
      var userConversation = new ConversationUser
      {
        ConversationId = conversationId,
        UserId = userId,
        LastMessageReadAt = DateTime.MinValue
      };
      listConversationUser.Add(userConversation);
    }

    _dbContext.ConversationUsers.AddRange(listConversationUser);
    await _dbContext.SaveChangesAsync();

    return listConversationUser.AsEnumerable();
  }

  public async Task<Conversation> CreateConversation(Conversation conversation)
  {
    var result = await _dbContext.Conversations.AddAsync(conversation);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public IEnumerable<Conversation> GetConversationUsers(Guid userId)
  {
    // get all conversations at ConversationUser table, sorted by lastMessage date
    var conversations = _dbContext.Conversations
      .Where(c => c.CreatedBy == userId)
      .OrderByDescending(c => c.UpdatedAt)
      .AsEnumerable();

    if (conversations == null)
    {
      return [];
    }

    return conversations;
  }

  public async Task<Conversation?> GetByIdAsync(Guid id)
  {
    var conversation = await _dbContext.Conversations
      .Include(c => c.CreatedByUser)
      .FirstOrDefaultAsync(c => c.Id == id);

    return conversation;
  }

  public async Task<int> UpdateLastSeen(Guid conversationId, Guid userId)
  {
    var rowsAffected = await _dbContext.Conversations
        .Where(uc => uc.Id == conversationId && uc.CreatedBy == userId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(b => b.UpdatedAt, DateTime.UtcNow));

    return rowsAffected;
  }

  public ConversationUser? GetConversationUser(Guid conversationId, Guid userId)
  {
    var userConversation = _dbContext.ConversationUsers
        .FirstOrDefault(uc => uc.ConversationId == conversationId && uc.UserId == userId);

    return userConversation;
  }

  public async Task<List<Guid>> GetConversationUserIds(Guid conversationId)
  {
    var users = await _dbContext.ConversationUsers
        .Where(uc => uc.ConversationId == conversationId)
        .Select(uc => uc.UserId)
        .ToListAsync();

    return users;
  }

  public async Task<Conversation?> GetConversationByUsers(Guid userId1, Guid userId2)
  {
    var conversation = await _dbContext.ConversationUsers
        .Where(uc => uc.UserId == userId1)
        .Join(_dbContext.ConversationUsers.Where(uc => uc.UserId == userId2),
            uc1 => uc1.ConversationId,
            uc2 => uc2.ConversationId,
            (uc1, uc2) => uc1.Conversation)
        .FirstOrDefaultAsync();

    return conversation;
  }

  public async Task<int> UpdateName(Guid conversationId, string name)
  {

    var rowsAffected = await _dbContext.Conversations
        .Where(uc => uc.Id == conversationId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(b => b.Name, name));

    return rowsAffected;
  }

  public async Task<Conversation?> UpdateConversation(Guid conversationId, UpdateConversation updateData)
  {
    var conversation = await _dbContext.Conversations.FindAsync(conversationId);
    if (conversation == null)
    {
      return null;
    }

    if (updateData.Name != null)
    {
      conversation.Name = updateData.Name;
    }

    if (updateData.Metadata != null)
    {
      conversation.Metadata = updateData.Metadata;
    }

    conversation.UpdatedAt = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync();
    return conversation;
  }

  public async Task<bool> DeleteConversation(Guid conversationId)
  {
    var conversation = await _dbContext.Conversations
        .Include(c => c.ConversationUsers)
        .FirstOrDefaultAsync(c => c.Id == conversationId);

    if (conversation == null)
    {
      return false;
    }

    _dbContext.ConversationUsers.RemoveRange(conversation.ConversationUsers);

    var messages = await _dbContext.Messages
        .Where(m => m.ConversationId == conversationId)
        .ToListAsync();
    _dbContext.Messages.RemoveRange(messages);

    _dbContext.Conversations.Remove(conversation);
    await _dbContext.SaveChangesAsync();

    return true;
  }
}