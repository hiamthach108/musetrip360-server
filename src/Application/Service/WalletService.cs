using Application.Service;
using Application.Shared.Enum;
using Application.Shared.Type;
using Application.DTOs.Payment;
using AutoMapper;
using Database;
using Domain.Payment;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

public interface IWalletService
{
    public Task InitWallet(Guid museumId);
    public Task AddBalance(Guid walletId, float amount);
    public Task<IActionResult> HandleGetWalletByMuseumId(Guid museumId);
    public Task<IActionResult> HandleCreatePayoutRequest(PayoutReq req);
    public Task<IActionResult> HandleElevatePayout(Guid payoutId, bool isApproved);
    public Task<IActionResult> HandleGetPayoutById(Guid id);
    public Task<IActionResult> HandleGetPayoutsByMuseumId(Guid museumId);
    public Task<IActionResult> HandleGetPayoutByStatus(PayoutStatusEnum status);
}
public class WalletService : BaseService, IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMuseumRepository _museumRepository;
    public WalletService(
        MuseTrip360DbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor
    )
        : base(dbContext, mapper, httpContextAccessor)
    {
        _walletRepository = new WalletRepository(dbContext);
        _museumRepository = new MuseumRepository(dbContext);
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

    public async Task<IActionResult> HandleGetWalletByMuseumId(Guid museumId)
    {
        try
        {
            var wallet = await _walletRepository.GetWalletByMuseumId(museumId);
            if (wallet == null)
            {
                return ErrorResp.NotFound("Wallet not found");
            }
            var result = _mapper.Map<WalletDto>(wallet);
            return SuccessResp.Ok(result);
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleCreatePayoutRequest(PayoutReq req)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            //check if auth of museum
            var isMuseumOwner = await _museumRepository.ValidateMuseumOwner(req.MuseumId, payload.UserId);
            if (!isMuseumOwner)
            {
                return ErrorResp.Unauthorized("You are not the owner of this museum");
            }
            //check if museum has enough balance
            var wallet = await _walletRepository.GetWalletByMuseumId(req.MuseumId);
            if (wallet == null)
            {
                return ErrorResp.NotFound("Wallet not found");
            }
            if (wallet.AvailableBalance < req.Amount)
            {
                return ErrorResp.BadRequest("Insufficient balance");
            }
            // hold wallet balance for payout
            await _walletRepository.HoldBalanceRequest(wallet.Id, req.Amount);
            //create payout
            var payout = _mapper.Map<Payout>(req);
            payout.ProcessedDate = DateTime.UtcNow;
            payout.Status = PayoutStatusEnum.Pending;
            await _walletRepository.CreatePayout(payout);

            await transaction.CommitAsync();
            return SuccessResp.Created(_mapper.Map<PayoutDto>(payout));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleElevatePayout(Guid payoutId, bool isApproved)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null || !payload.IsAdmin)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            //check if payout exists
            var payout = await _walletRepository.GetPayoutById(payoutId);
            if (payout == null)
            {
                return ErrorResp.NotFound("Payout not found");
            }
            //check if payout is pending
            if (payout.Status != PayoutStatusEnum.Pending)
            {
                return ErrorResp.BadRequest("Payout is not pending");
            }
            //update payout status
            payout.Status = isApproved ? PayoutStatusEnum.Approved : PayoutStatusEnum.Rejected;
            await _walletRepository.UpdatePayout(payout);
            return SuccessResp.Ok(_mapper.Map<PayoutDto>(payout));
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleGetPayoutById(Guid id)
    {
        try
        {
            var payout = await _walletRepository.GetPayoutById(id);
            if (payout == null)
            {
                throw new Exception("Payout not found");
            }
            return SuccessResp.Ok(_mapper.Map<PayoutDto>(payout));
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleGetPayoutsByMuseumId(Guid museumId)
    {
        try
        {
            var payouts = await _walletRepository.GetPayoutsByMuseumId(museumId);
            return SuccessResp.Ok(_mapper.Map<List<PayoutDto>>(payouts));
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleGetPayoutByStatus(PayoutStatusEnum status)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null || !payload.IsAdmin)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            var payouts = await _walletRepository.GetPayoutsByStatus(status);
            if (payouts == null)
            {
                return ErrorResp.NotFound("Payouts not found");
            }
            return SuccessResp.Ok(_mapper.Map<List<PayoutDto>>(payouts));
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }
}