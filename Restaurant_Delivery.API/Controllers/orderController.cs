using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Delivery.API.BusinessLogicLayer.Services;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

[Authorize]
[ApiController]
[Route("api/order")]
public class OrderController : ControllerBase {
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;
    private readonly ITokenService _tokenService;

    public OrderController(
        IOrderService orderService,
        ILogger<OrderController> logger,
        ITokenService tokenService) {
        _orderService = orderService;
        _logger = logger;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Get information about a specific order.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrder(Guid id) {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to retrieve order {OrderId}.", id);
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            var order = await _orderService.GetOrderAsync(id, userId);
            return Ok(order);
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "Order {OrderId} not found.", id);
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while retrieving order {OrderId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = "An unexpected error occurred while retrieving the order."
            });
        }
    }

    /// <summary>
    /// Get a list of orders.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrders() {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to retrieve orders.");
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            var orders = await _orderService.GetOrdersAsync(userId);
            return Ok(orders);
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while retrieving orders.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = "An unexpected error occurred while retrieving orders."
            });
        }
    }

    /// <summary>
    /// Create an order from dishes in the basket.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto request) {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to create an order.");
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            var orderId = await _orderService.CreateOrderAsync(userId, request);
            return Ok(new { Status = "Success", OrderId = orderId });
        } catch (ArgumentException ex) {
            _logger.LogWarning(ex, "Invalid request data for creating an order.");
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (InvalidOperationException ex) {
            _logger.LogWarning(ex, "Attempt to create an order with an empty basket.");
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while creating an order.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = "An unexpected error occurred while creating the order."
            });
        }
    }

    /// <summary>
    /// Confirm order delivery.
    /// </summary>
    [HttpPost("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmDelivery(Guid id) {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to confirm delivery for order {OrderId}.", id);
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            await _orderService.ConfirmDeliveryAsync(id, userId);
            return Ok(new { Status = "Success", Message = "Order marked as delivered." });
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "Order {OrderId} not found.", id);
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (InvalidOperationException ex) {
            _logger.LogWarning(ex, "Order {OrderId} is already delivered.", id);
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while confirming delivery for order {OrderId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = "An unexpected error occurred while confirming delivery."
            });
        }
    }
}