using Database;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public interface IWalletRepository
{
    public Task<MuseumWallet> InitWallet(Guid museumId);
    //public Task UpdateWallet(Guid museumId, float amount);
    public Task<MuseumWallet?> GetWalletByMuseumId(Guid museumId);
    public Task AddBalance(Guid walletId, float amount);
    //public Task SubtractBalance(Guid museumId, float amount);
    public Task<MuseumWallet?> GetById(Guid walletId);
}

public class WalletRepository : IWalletRepository
{
    private readonly MuseTrip360DbContext _dbContext;

    public WalletRepository(MuseTrip360DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MuseumWallet> InitWallet(Guid museumId)
    {
        var wallet = new MuseumWallet
        {
            MuseumId = museumId,
            AvailableBalance = 0,
            PendingBalance = 0,
            TotalBalance = 0
        };
        var result = await _dbContext.MuseumWallets.AddAsync(wallet);
        await _dbContext.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<MuseumWallet?> GetWalletByMuseumId(Guid museumId)
    {
        return await _dbContext.MuseumWallets.FindAsync(museumId);
    }

    public async Task AddBalance(Guid walletId, float amount)
    {
        var wallet = await GetById(walletId);
        if (wallet == null) return;

        wallet.AvailableBalance += amount;
        wallet.TotalBalance += amount;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<MuseumWallet?> GetById(Guid walletId)
    {
        return await _dbContext.MuseumWallets.FindAsync(walletId);
    }
}