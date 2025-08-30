using Application.Service;
using Application.Shared.Enum;
using Application.Shared.Type;
using Application.DTOs.Payment;
using AutoMapper;
using Database;
using Domain.Payment;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc;
using Application.Shared.Constant;

public interface IWalletService
{
    public Task InitWallet(Guid museumId);
    public Task AddBalance(Guid walletId, float amount);
    public Task<IActionResult> HandleGetWalletByMuseumId(Guid museumId);
    public Task<IActionResult> HandleCreatePayoutRequest(PayoutReq req);
    public Task<IActionResult> HandleElevatePayout(Guid payoutId, EvaluatePayoutReq req, bool isApproved);
    public Task<IActionResult> HandleGetPayoutById(Guid id);
    public Task<IActionResult> HandleGetPayoutsByMuseumId(Guid museumId);
    public Task<IActionResult> HandleGetPayoutByStatus(PayoutStatusEnum status);
    public Task<IActionResult> HandleCreateMuseumWallet(Guid museumId);
    public Task<IActionResult> HandleGetPayoutsAdmin(PayoutQuery query);
}
public class WalletService : BaseService, IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IMuseumRepository _museumRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUserService _userSvc;
    public WalletService(
        MuseTrip360DbContext dbContext,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IUserService userService
    )
        : base(dbContext, mapper, httpContextAccessor)
    {
        _walletRepository = new WalletRepository(dbContext);
        _museumRepository = new MuseumRepository(dbContext);
        _bankAccountRepository = new BankAccountRepository(dbContext);
        _userSvc = userService;
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
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var payload = ExtractPayload();
                if (payload == null)
                {
                    return ErrorResp.Unauthorized("Unauthorized");
                }
                var isAllowed = await _userSvc.ValidatePermission(req.MuseumId.ToString(), [PermissionConst.PAYMENT_MANAGEMENT]);
                if (!isAllowed)
                {
                    return ErrorResp.Forbidden("You are not allowed to access this resource");
                }
                //check if museum has enough balance
                var wallet = await _walletRepository.GetWalletByMuseumId(req.MuseumId);
                if (wallet == null)
                {
                    return ErrorResp.NotFound("Wallet not found");
                }
                var museum = await _museumRepository.GetByIdAsync(req.MuseumId);
                if (museum == null)
                {
                    return ErrorResp.NotFound("Museum not found");
                }
                var backAccount = await _bankAccountRepository.GetByIdAsync(req.BankAccountId);
                if (backAccount == null)
                {
                    return ErrorResp.NotFound("Bank account not found");
                }
                if (wallet.AvailableBalance < req.Amount)
                {
                    return ErrorResp.BadRequest("Insufficient balance");
                }
                // hold wallet balance for payout
                await _walletRepository.HoldBalanceRequest(wallet.Id, req.Amount);
                //create payout
                var payout = _mapper.Map<Payout>(req);
                payout.BankAccountId = backAccount.Id;
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
    }

    public async Task<IActionResult> HandleElevatePayout(Guid payoutId, EvaluatePayoutReq req, bool isApproved)
    {
        await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
        {
            try
            {
                var payload = ExtractPayload();
                if (payload == null)
                {
                    return ErrorResp.Unauthorized("Unauthorized");
                }
                var isAllowed = await _userSvc.ValidatePermission(PermissionConst.SYSTEM_MUSEUM, [PermissionConst.PAYMENT_MANAGEMENT]);
                if (!isAllowed)
                {
                    return ErrorResp.Forbidden("You are not allowed to access this resource");
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
                payout.ProcessedDate = DateTime.UtcNow;
                //check if wallet exists
                var wallet = await _walletRepository.GetWalletByMuseumId(payout.MuseumId);
                if (wallet == null)
                {
                    return ErrorResp.NotFound("Wallet not found");
                }
                if (isApproved)
                {
                    await _walletRepository.WithdrawBalance(wallet.Id);
                    payout.Status = PayoutStatusEnum.Approved;
                }
                else
                {
                    await _walletRepository.RejectPayout(wallet.Id);
                    payout.Status = PayoutStatusEnum.Rejected;
                }
                payout.Metadata = req.Metadata;
                await _walletRepository.UpdatePayout(payout);
                await transaction.CommitAsync();
                return SuccessResp.Ok("Payout updated successfully");
            }
            catch (Exception ex)
            {
                return ErrorResp.InternalServerError(ex.Message);
            }
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
            if (payload == null)
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

    public async Task<IActionResult> HandleCreateMuseumWallet(Guid museumId)
    {
        try
        {
            var museum = await _museumRepository.GetByIdAsync(museumId);
            if (museum == null)
            {
                return ErrorResp.NotFound("Museum not found");
            }
            var wallet = await _walletRepository.GetWalletByMuseumId(museumId);
            if (wallet != null)
            {
                return ErrorResp.BadRequest("Wallet already exists");
            }
            await _walletRepository.InitWallet(museumId);
            return SuccessResp.Ok("Wallet created successfully");
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }

    public async Task<IActionResult> HandleGetPayoutsAdmin(PayoutQuery query)
    {
        try
        {
            var payload = ExtractPayload();
            if (payload == null)
            {
                return ErrorResp.Unauthorized("Unauthorized");
            }
            var isAllowed = await _userSvc.ValidatePermission(PermissionConst.SYSTEM_MUSEUM, [PermissionConst.PAYMENT_MANAGEMENT]);
            if (!isAllowed)
            {
                return ErrorResp.Forbidden("You are not allowed to access this resource");
            }
            var payouts = await _walletRepository.GetPayoutsAdmin(query);
            var payoutDto = _mapper.Map<List<PayoutDto>>(payouts.Payouts);
            return SuccessResp.Ok(new
            {
                Payouts = payoutDto,
                payouts.Total
            });
        }
        catch (Exception ex)
        {
            return ErrorResp.InternalServerError(ex.Message);
        }
    }
}