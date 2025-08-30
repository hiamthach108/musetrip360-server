namespace Application.Controllers;

using Application.DTOs.Payment;
using Application.Middlewares;
using Application.Service;
using Application.Shared.Enum;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing museum wallets, balances, and payout operations
/// </summary>
[ApiController]
[Route("/api/v1/wallets")]
[Produces("application/json")]
public class WalletController : ControllerBase
{
    private readonly ILogger<WalletController> _logger;
    private readonly IWalletService _walletService;

    /// <summary>
    /// Initializes a new instance of the WalletController
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="walletService">The wallet service for business logic</param>
    public WalletController(ILogger<WalletController> logger, IWalletService walletService)
    {
        _logger = logger;
        _walletService = walletService;
    }

    /// <summary>
    /// Get wallet information for a specific museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <returns>Wallet information including balance details</returns>
    /// <response code="200">Returns the wallet information</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Wallet not found for the specified museum</response>
    [Protected]
    [HttpGet("museum/{museumId}")]
    public async Task<IActionResult> GetWalletByMuseumId(Guid museumId)
    {
        _logger.LogInformation("Get wallet by museum id request received for museum: {MuseumId}", museumId);
        return await _walletService.HandleGetWalletByMuseumId(museumId);
    }

    /// <summary>
    /// Create a new payout request for a museum
    /// </summary>
    /// <param name="req">The payout request details including museum ID, bank account, and amount</param>
    /// <returns>Created payout information</returns>
    /// <response code="201">Payout request created successfully</response>
    /// <response code="400">Bad request - Invalid payout details or insufficient balance</response>
    /// <response code="401">Unauthorized - User is not authenticated or not the museum owner</response>
    /// <response code="404">Wallet or museum not found</response>
    [Protected]
    [HttpPost("payouts")]
    public async Task<IActionResult> CreatePayoutRequest([FromBody] PayoutReq req)
    {
        _logger.LogInformation("Create payout request received for museum: {MuseumId}, amount: {Amount}", req.MuseumId, req.Amount);
        return await _walletService.HandleCreatePayoutRequest(req);
    }

    /// <summary>
    /// Approve a pending payout request (Admin only)
    /// </summary>
    /// <param name="payoutId">The unique identifier of the payout to approve</param>
    /// <param name="req">The request body containing the metadata</param>
    /// <returns>Updated payout information</returns>
    /// <response code="200">Payout approved successfully</response>
    /// <response code="400">Bad request - Payout is not in pending status</response>
    /// <response code="401">Unauthorized - User is not authenticated or not an admin</response>
    /// <response code="404">Payout not found</response>
    [Protected]
    [HttpPut("payouts/{payoutId}/approve")]
    public async Task<IActionResult> ApprovePayout(Guid payoutId, [FromBody] EvaluatePayoutReq req)
    {
        return await _walletService.HandleElevatePayout(payoutId, req, true);
    }

    /// <summary>
    /// Reject a pending payout request (Admin only)
    /// </summary>
    /// <param name="payoutId">The unique identifier of the payout to reject</param>
    /// <param name="req">The request body containing the metadata</param>
    /// <returns>Updated payout information</returns>
    /// <response code="200">Payout rejected successfully</response>
    /// <response code="400">Bad request - Payout is not in pending status</response>
    /// <response code="401">Unauthorized - User is not authenticated or not an admin</response>
    /// <response code="404">Payout not found</response>
    [Protected]
    [HttpPut("payouts/{payoutId}/reject")]
    public async Task<IActionResult> RejectPayout(Guid payoutId, [FromBody] EvaluatePayoutReq req)
    {
        return await _walletService.HandleElevatePayout(payoutId, req, false);
    }

    /// <summary>
    /// Get payout information by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the payout</param>
    /// <returns>Payout information</returns>
    /// <response code="200">Returns the payout information</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Payout not found</response>
    [Protected]
    [HttpGet("payouts/{id}")]
    public async Task<IActionResult> GetPayoutById(Guid id)
    {
        _logger.LogInformation("Get payout by id request received for payout: {Id}", id);
        return await _walletService.HandleGetPayoutById(id);
    }

    /// <summary>
    /// Get all payouts for a specific museum
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <returns>List of payouts for the specified museum</returns>
    /// <response code="200">Returns the list of payouts</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    [Protected]
    [HttpGet("payouts/museum/{museumId}")]
    public async Task<IActionResult> GetPayoutsByMuseumId(Guid museumId)
    {
        _logger.LogInformation("Get payouts by museum id request received for museum: {MuseumId}", museumId);
        return await _walletService.HandleGetPayoutsByMuseumId(museumId);
    }

    /// <summary>
    /// Get all payouts by status (Admin only)
    /// </summary>
    /// <param name="status">The status to filter payouts by (Pending, Approved, Rejected)</param>
    /// <returns>List of payouts with the specified status</returns>
    /// <response code="200">Returns the list of payouts</response>
    /// <response code="401">Unauthorized - User is not authenticated or not an admin</response>
    /// <response code="404">No payouts found with the specified status</response>
    [Protected]
    [HttpGet("payouts/status/{status}")]
    public async Task<IActionResult> GetPayoutsByStatus(PayoutStatusEnum status)
    {
        _logger.LogInformation("Get payouts by status request received for status: {Status}", status);
        return await _walletService.HandleGetPayoutByStatus(status);
    }

    /// <summary>
    /// Create wallet for museums
    /// </summary>
    /// <param name="museumId">The unique identifier of the museum</param>
    /// <returns>Wallet information including balance details</returns>
    /// <response code="200">Returns the wallet information</response>
    /// <response code="401">Unauthorized - User is not authenticated</response>
    /// <response code="404">Wallet not found for the specified museum</response>
    [Protected]
    [HttpPost("museum/{museumId}")]
    public async Task<IActionResult> CreateMuseumWallet(Guid museumId)
    {
        _logger.LogInformation("Create museum wallet request received for museum: {MuseumId}", museumId);
        return await _walletService.HandleCreateMuseumWallet(museumId);
    }
    /// <summary>
    /// Get all payouts (Admin only)
    /// </summary>
    /// <param name="query">The query parameters for filtering payouts</param>
    /// <returns>List of payouts with the specified status</returns>
    /// <response code="200">Returns the list of payouts</response>
    /// <response code="401">Unauthorized - User is not authenticated or not an admin</response>
    /// <response code="404">No payouts found with the specified status</response>
    [Protected]
    [HttpGet("payouts/admin")]
    public async Task<IActionResult> GetPayoutsAdmin([FromQuery] PayoutQuery query)
    {
        _logger.LogInformation("Get payouts admin request received for query: {Query}", query);
        return await _walletService.HandleGetPayoutsAdmin(query);
    }
}
