namespace Infrastructure.Repository;

using Application.DTOs.Payment;
using Database;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;

public interface IBankAccountRepository
{
  Task<BankAccount?> GetByIdAsync(Guid id);
  Task<IEnumerable<BankAccount>> GetAllAsync();
  Task<IEnumerable<BankAccount>> GetByMuseumIdAsync(Guid museumId);
  Task<BankAccount> AddAsync(BankAccount bankAccount);
  Task<BankAccount?> UpdateAsync(Guid id, BankAccount bankAccount);
  Task<BankAccount?> DeleteAsync(Guid id);
  Task<bool> ExistsAccountNumberForMuseumAsync(string accountNumber, Guid museumId, Guid? excludeId = null);
}

public class BankAccountRepository : IBankAccountRepository
{
  private readonly MuseTrip360DbContext _dbContext;

  public BankAccountRepository(MuseTrip360DbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<BankAccount> AddAsync(BankAccount bankAccount)
  {
    var result = await _dbContext.BankAccounts.AddAsync(bankAccount);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<BankAccount?> DeleteAsync(Guid id)
  {
    var bankAccount = await _dbContext.BankAccounts.FindAsync(id);
    if (bankAccount == null) return null;

    var result = _dbContext.BankAccounts.Remove(bankAccount);
    await _dbContext.SaveChangesAsync();
    return result.Entity;
  }

  public async Task<IEnumerable<BankAccount>> GetAllAsync()
  {
    return await _dbContext.BankAccounts
      .Include(b => b.Museum)
      .ToListAsync();
  }

  public async Task<BankAccount?> GetByIdAsync(Guid id)
  {
    return await _dbContext.BankAccounts
      .Include(b => b.Museum)
      .FirstOrDefaultAsync(b => b.Id == id);
  }

  public async Task<IEnumerable<BankAccount>> GetByMuseumIdAsync(Guid museumId)
  {
    return await _dbContext.BankAccounts
      .Where(b => b.MuseumId == museumId)
      .Include(b => b.Museum)
      .ToListAsync();
  }

  public async Task<BankAccount?> UpdateAsync(Guid id, BankAccount bankAccount)
  {
    var existing = await _dbContext.BankAccounts.FindAsync(id);
    if (existing == null) return null;

    _dbContext.Entry(existing).CurrentValues.SetValues(bankAccount);
    existing.UpdatedAt = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync();
    return existing;
  }

  public async Task<bool> ExistsAccountNumberForMuseumAsync(string accountNumber, Guid museumId, Guid? excludeId = null)
  {
    var query = _dbContext.BankAccounts
      .Where(b => b.AccountNumber == accountNumber && b.MuseumId == museumId);

    if (excludeId.HasValue)
    {
      query = query.Where(b => b.Id != excludeId.Value);
    }

    return await query.AnyAsync();
  }
}