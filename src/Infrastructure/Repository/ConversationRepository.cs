namespace Infrastructure.Repository;

using System;
using Database;
using Domain.Messaging;
using Microsoft.EntityFrameworkCore;

public interface IConversationRepository
{
  // Conversation
  Task<Conversation?> GetByIdAsync(Guid id);
  IEnumerable<ConversationUser> GetConversationUsers(Guid userId);
  ConversationUser? GetConversationUser(Guid conversationId, Guid userId);
  Task<Conversation> CreateConversation(Conversation conversation);
  Task<IEnumerable<ConversationUser>> AddUsersToConversation(Guid conversationId, List<Guid> userIds);
  Task<int> UpdateLastSeen(Guid conversationId, Guid userId);
  Task<int> UpdateLastMessage(Guid conversationId, Guid messageId);
  Task<int> UpdateName(Guid conversationId, string name);
  Task<List<Guid>> GetConversationUserIds(Guid conversationId);
  Task<Conversation> GetConversationByUsers(Guid userId1, Guid userId2);
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

  public IEnumerable<ConversationUser> GetConversationUsers(Guid userId)
  {
    // get all conversations at ConversationUser table, sorted by lastMessage date
    var conversations = _dbContext.ConversationUsers
      .Where(uc => uc.UserId == userId)
      .Include(uc => uc.Conversation)
      .Include(uc => uc.Conversation.LastMessage)
      .Include(uc => uc.Conversation.LastMessage != null ? uc.Conversation.LastMessage.CreatedByUser : null)
      .OrderByDescending(uc => uc.Conversation.LastMessage != null
          ? uc.Conversation.LastMessage.CreatedAt
          : uc.Conversation.CreatedAt)
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

  public async Task<int> UpdateLastMessage(Guid conversationId, Guid messageId)
  {
    var rowsAffected = await _dbContext.Conversations
        .Where(uc => uc.Id == conversationId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(b => b.LastMessageId, messageId));

    return rowsAffected;
  }

  public async Task<int> UpdateLastSeen(Guid conversationId, Guid userId)
  {
    var rowsAffected = await _dbContext.ConversationUsers
        .Where(uc => uc.ConversationId == conversationId && uc.UserId == userId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(b => b.LastMessageReadAt, DateTime.UtcNow));

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
}