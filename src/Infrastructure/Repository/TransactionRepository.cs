using Database;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;

public interface ITransactionRepository
{
    public Task CreateTransaction(Transaction transaction);
    public Task<Transaction?> GetTransactionById(Guid id);
    public Task<List<Transaction>> GetTransactionsByReferenceId(string referenceId, string transactionType);
}
public class TransactionRepository : ITransactionRepository
{
    private readonly MuseTrip360DbContext _dbContext;

    public TransactionRepository(MuseTrip360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreateTransaction(Transaction transaction)
    {
        await _dbContext.Transactions.AddAsync(transaction);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Transaction?> GetTransactionById(Guid id)
    {
        return await _dbContext.Transactions.FindAsync(id);
    }

    public async Task<List<Transaction>> GetTransactionsByReferenceId(string referenceId, string transactionType)
    {
        return await _dbContext.Transactions
        .Where(t => t.ReferenceId == referenceId && t.TransactionType == transactionType)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
    }
}