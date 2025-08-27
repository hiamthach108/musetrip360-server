namespace Application.Controllers;

using Application.DTOs.Payment;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/api/v1/bank-accounts")]
public class BankAccountController : ControllerBase
{
    private readonly ILogger<BankAccountController> _logger;
    private readonly IPaymentService _service;

    public BankAccountController(ILogger<BankAccountController> logger, IPaymentService service)
    {
        _logger = logger;
        _service = service;
    }

    [Protected]
    [HttpGet("museum/{museumId}")]
    public async Task<IActionResult> GetByMuseum(Guid museumId)
    {
        _logger.LogInformation("Get bank accounts by museum request received: {MuseumId}", museumId);
        return await _service.HandleGetBankAccountsByMuseum(museumId);
    }

    [Protected]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Get bank account by id request received: {Id}", id);
        return await _service.HandleGetBankAccountById(id);
    }

    [Protected]
    [HttpPost("museum/{museumId}")]
    public async Task<IActionResult> Create(Guid museumId, [FromBody] BankAccountCreateDto dto)
    {
        _logger.LogInformation("Create bank account request received for museum: {MuseumId}", museumId);
        return await _service.HandleCreateBankAccount(museumId, dto);
    }

    [Protected]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] BankAccountUpdateDto dto)
    {
        _logger.LogInformation("Update bank account request received: {Id}", id);
        return await _service.HandleUpdateBankAccount(id, dto);
    }

    [Protected]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Delete bank account request received: {Id}", id);
        return await _service.HandleDeleteBankAccount(id);
    }
}
