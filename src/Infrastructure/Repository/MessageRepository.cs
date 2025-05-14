namespace Infrastructure.Repository;

using Database;
using Domain.Messaging;
using Microsoft.EntityFrameworkCore;

public interface IMessageRepository
{
  Task<Message> CreateMessage(Message message);
  Task<IEnumerable<Message>> CreateMessages(List<Message> messages);
  MessageList GetConversationMessages(Guid conversationId, int page, int pageSize);
}

public class MessageList
{
  public IEnumerable<Message> Messages { get; set; } = [];
  public int Total { get; set; }
}

public class MessageRepository : IMessageRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public MessageRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Message> CreateMessage(Message message)
  {
    var result = await _dbContext.Messages.AddAsync(message);
    await _dbContext.SaveChangesAsync();

    // Fetch the message with its related user
    return _dbContext.Messages
      .Include(m => m.CreatedByUser)
      .FirstOrDefault(m => m.Id == result.Entity.Id) ?? result.Entity;
  }

  public async Task<IEnumerable<Message>> CreateMessages(List<Message> messages)
  {
    _dbContext.Messages.AddRange(messages);
    await _dbContext.SaveChangesAsync();
    return messages.AsEnumerable();
  }

  public MessageList GetConversationMessages(Guid conversationId, int page, int pageSize)
  {
    var total = _dbContext.Messages.Count(x => x.ConversationId == conversationId);

    var messages = _dbContext.Messages
      .Include(x => x.CreatedByUser)
      .Where(x => x.ConversationId == conversationId)
      .OrderByDescending(x => x.CreatedAt)
      .Skip(page * pageSize)
      .Take(pageSize)
      .AsEnumerable();

    return new MessageList { Messages = messages, Total = total };
  }
}