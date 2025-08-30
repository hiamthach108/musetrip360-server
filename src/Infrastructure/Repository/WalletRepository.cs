using System.Text.Json;
using Application.Shared.Enum;
using Database;
using Domain.Museums;
using Domain.Payment;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository;

public interface IWalletRepository
{
    public Task<MuseumWallet> InitWallet(Guid museumId);
    public Task HoldBalanceRequest(Guid walletId, float amount);
    public Task WithdrawBalance(Guid walletId);
    public Task RejectPayout(Guid walletId);
    public Task<MuseumWallet?> GetWalletByMuseumId(Guid museumId);
    public Task AddBalance(Guid walletId, float amount);
    //public Task SubtractBalance(Guid museumId, float amount);
    public Task<MuseumWallet?> GetById(Guid walletId);
    public Task CreatePayout(Payout payout);
    public Task UpdatePayout(Payout payout);
    public Task<Payout?> GetPayoutById(Guid id);
    public Task<List<Payout>> GetPayoutsByMuseumId(Guid museumId);
    public Task<List<Payout>> GetPayoutsByStatus(PayoutStatusEnum status);
    public Task<PayoutQueryResp> GetPayoutsAdmin(PayoutQuery query);

}

public class PayoutQueryResp
{
    public List<Payout> Payouts { get; set; } = new List<Payout>();
    public int Total { get; set; }
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
        return await _dbContext.MuseumWallets
        .Where(w => w.MuseumId == museumId)
        .FirstOrDefaultAsync();
    }

    public async Task AddBalance(Guid walletId, float amount)
    {
        var wallet = await GetById(walletId);
        if (wallet == null) return;
        // update both avail balance and total
        wallet.AddBalance(amount);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<MuseumWallet?> GetById(Guid walletId)
    {
        return await _dbContext.MuseumWallets
        .FindAsync(walletId);
    }
    public async Task CreatePayout(Payout payout)
    {
        await _dbContext.Payouts.AddAsync(payout);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdatePayout(Payout payout)
    {
        var existingPayout = await _dbContext.Payouts.FindAsync(payout.Id);
        if (existingPayout == null) return;
        _dbContext.Entry(existingPayout).CurrentValues.SetValues(payout);
        await _dbContext.SaveChangesAsync();
    }



    public async Task<Payout?> GetPayoutById(Guid id)
    {
        return await _dbContext.Payouts
        .Where(p => p.Id == id)
        .Select(p => new Payout
        {
            Id = p.Id,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            MuseumId = p.MuseumId,
            BankAccountId = p.BankAccountId,
            Museum = new Museum
            {
                Id = p.Museum.Id,
                Name = p.Museum.Name,
                Location = p.Museum.Location,
                ContactEmail = p.Museum.ContactEmail,
                ContactPhone = p.Museum.ContactPhone,

            },
            BankAccount = new BankAccount
            {
                Id = p.BankAccount.Id,
                HolderName = p.BankAccount.HolderName,
                BankName = p.BankAccount.BankName,
                AccountNumber = p.BankAccount.AccountNumber,
                QRCode = p.BankAccount.QRCode

            }
        })
        .AsNoTracking()
        .OrderByDescending(p => p.CreatedAt)
        .FirstOrDefaultAsync();
    }

    public async Task<List<Payout>> GetPayoutsByMuseumId(Guid museumId)
    {
        return await _dbContext.Payouts.Where(p => p.MuseumId == museumId)
        .OrderByDescending(p => p.CreatedAt)
        .Select(p => new Payout
        {
            Id = p.Id,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            Museum = new Museum
            {
                Id = p.Museum.Id,
                Name = p.Museum.Name,
                Location = p.Museum.Location,
                ContactEmail = p.Museum.ContactEmail,
                ContactPhone = p.Museum.ContactPhone,
            },
            BankAccount = new BankAccount
            {
                Id = p.BankAccount.Id,
                HolderName = p.BankAccount.HolderName,
                BankName = p.BankAccount.BankName,
                AccountNumber = p.BankAccount.AccountNumber,
                QRCode = p.BankAccount.QRCode
            }
        })
        .AsNoTracking()
        .ToListAsync();
    }

    public async Task<List<Payout>> GetPayoutsByStatus(PayoutStatusEnum status)
    {
        return await _dbContext.Payouts.Where(p => p.Status == status)
        .Select(p => new Payout
        {
            Id = p.Id,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            Museum = new Museum
            {
                Id = p.Museum.Id,
                Name = p.Museum.Name,
                Location = p.Museum.Location,
                ContactEmail = p.Museum.ContactEmail,
                ContactPhone = p.Museum.ContactPhone,
            },
            BankAccount = new BankAccount
            {
                Id = p.BankAccount.Id,
                HolderName = p.BankAccount.HolderName,
                BankName = p.BankAccount.BankName,
                AccountNumber = p.BankAccount.AccountNumber,
                QRCode = p.BankAccount.QRCode
            }
        })
        .AsNoTracking()
        .OrderByDescending(p => p.CreatedAt)
        .ToListAsync();
    }

    public async Task HoldBalanceRequest(Guid walletId, float amount)
    {
        var wallet = await GetById(walletId);
        if (wallet == null) return;
        wallet.HoldBalance(amount);
        await _dbContext.SaveChangesAsync();
    }

    public async Task WithdrawBalance(Guid walletId)
    {
        var wallet = await GetById(walletId);
        if (wallet == null) return;
        wallet.WithdrawBalance();
        await _dbContext.SaveChangesAsync();
    }

    public async Task RejectPayout(Guid walletId)
    {
        var wallet = await GetById(walletId);
        if (wallet == null) return;
        wallet.RejectPayout();
        await _dbContext.SaveChangesAsync();
    }

    public async Task<PayoutQueryResp> GetPayoutsAdmin(PayoutQuery req)
    {
        var query = _dbContext.Payouts.AsQueryable();
        if (req.MuseumId != null)
        {
            query = query.Where(p => p.MuseumId == req.MuseumId);
        }
        if (req.BankAccountId != null)
        {
            query = query.Where(p => p.BankAccountId == req.BankAccountId);
        }
        if (req.AmountFrom != null && req.AmountTo != null)
        {
            query = query.Where(p => p.Amount >= req.AmountFrom && p.Amount <= req.AmountTo);
        }
        if (req.CreatedAtFrom != null && req.CreatedAtTo != null)
        {
            query = query.Where(p => p.CreatedAt >= req.CreatedAtFrom && p.CreatedAt <= req.CreatedAtTo);
        }
        if (req.Status != null)
        {
            query = query.Where(p => p.Status == req.Status);
        }
        var total = await query.CountAsync();
        var payoutList = await query.OrderByDescending(p => p.CreatedAt)
        .Skip((req.Page - 1) * req.PageSize)
        .Take(req.PageSize)
        .Select(p => new Payout
        {
            Id = p.Id,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            Museum = new Museum
            {
                Id = p.Museum.Id,
                Name = p.Museum.Name,
                Location = p.Museum.Location,
                ContactEmail = p.Museum.ContactEmail,
                ContactPhone = p.Museum.ContactPhone,
            },
            BankAccount = new BankAccount
            {
                Id = p.BankAccount.Id,
                HolderName = p.BankAccount.HolderName,
                BankName = p.BankAccount.BankName,
                AccountNumber = p.BankAccount.AccountNumber,
                QRCode = p.BankAccount.QRCode
            }
        })
        .AsNoTracking()
        .ToListAsync();

        return new PayoutQueryResp
        {
            Payouts = payoutList,
            Total = total
        };
    }
}