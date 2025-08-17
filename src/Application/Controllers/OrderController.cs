namespace MuseTrip360.Controllers;

using Application.DTOs.Order;
using Application.DTOs.Payment;
using Application.Middlewares;
using Application.Service;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

[ApiController]
[Route("/api/v1/orders")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IPaymentService _service;

    public OrderController(ILogger<OrderController> logger, IPaymentService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Creates a new order for events, tours, or subscriptions
    /// </summary>
    /// <param name="req">Order creation request containing total amount, order type, metadata, and item IDs</param>
    /// <returns>Payment information including checkout URL, order code, and payment details</returns>
    /// <response code="200">Order created successfully with payment information</response>
    /// <response code="401">Unauthorized - Invalid or missing access token</response>
    /// <response code="400">Bad request - Invalid order data</response>
    [Protected]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderReq req)
    {
        _logger.LogInformation("Create order request received");

        return await _service.HandleCreateOrder(req);
    }

    /// <summary>
    /// Retrieves a specific order by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the order</param>
    /// <returns>Order details including status, amount, and metadata</returns>
    /// <response code="200">Order details retrieved successfully</response>
    /// <response code="401">Unauthorized - Invalid or missing access token</response>
    /// <response code="404">Order not found</response>
    [Protected]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        _logger.LogInformation("Get order by id request received");

        return await _service.HandleGetOrderById(id);
    }

    /// <summary>
    /// Retrieves all orders for the authenticated user with optional filtering and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering, sorting, and pagination</param>
    /// <returns>List of user's orders with pagination information</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">Unauthorized - Invalid or missing access token</response>
    [Protected]
    [HttpGet]
    public async Task<IActionResult> GetOrdersByUser([FromQuery] OrderQuery query)
    {
        _logger.LogInformation("Get orders by user request received");

        return await _service.HandleGetOrdersByUser(query);
    }

    /// <summary>
    /// Retrieves all orders in the system for administrative purposes
    /// </summary>
    /// <param name="query">Query parameters for filtering, sorting, and pagination</param>
    /// <returns>List of all orders in the system with pagination information</returns>
    /// <response code="200">Orders retrieved successfully</response>
    /// <response code="401">Unauthorized - Invalid or missing access token</response>
    /// <response code="403">Forbidden - Insufficient permissions for admin access</response>
    [Protected]
    [HttpGet("admin")]
    public async Task<IActionResult> GetOrdersForAdmin([FromQuery] OrderAdminQuery query)
    {
        _logger.LogInformation("Get orders for admin request received");

        return await _service.HandleAdminGetOrders(query);
    }

    [HttpPost("payos-webhook")]
    public async Task<IActionResult> PayosWebhook([FromBody] WebhookType data)
    {
        _logger.LogInformation("Callback request received");

        return await _service.HandlePayosWebhook(data);
    }

    [HttpGet("code/{orderCode}")]
    public async Task<IActionResult> GetOrderByCode(string orderCode)
    {
        _logger.LogInformation("Callback request received");

        return await _service.HandleGetOrderByCode(orderCode);
    }
}
