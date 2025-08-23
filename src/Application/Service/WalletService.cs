using Application.Service;
using AutoMapper;
using Database;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;

public interface IWalletService
{
    public Task InitWallet(Guid museumId);
    public Task AddBalance(Guid walletId, float amount);
}
public class WalletService : BaseService, IWalletService
{
    private readonly IWalletRepository _walletRepository;

    public WalletService(
        MuseTrip360DbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor
    )
        : base(dbContext, mapper, httpContextAccessor)
    {
        _walletRepository = new WalletRepository(dbContext);
    }

    public async Task InitWallet(Guid museumId)
    {
        await _walletRepository.InitWallet(museumId);
    }

    public async Task AddBalance(Guid walletId, float amount)
    {
        using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var wallet = await _walletRepository.GetById(walletId);
                if (wallet == null)
                {
                    throw new Exception("Wallet not found");
                }
                await _walletRepository.AddBalance(wallet.Id, amount);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}