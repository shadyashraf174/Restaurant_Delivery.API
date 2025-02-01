using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Delivery.API.BusinessLogicLayer.Services;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

[ApiController]
[Route("api/basket")]
[Authorize]
public class BasketController : ControllerBase {
    private readonly IBasketService _basketService;
    private readonly ILogger<BasketController> _logger;
    private readonly ITokenService _tokenService;

    public BasketController(
        IBasketService basketService,
        ILogger<BasketController> logger,
        ITokenService tokenService) {
        _basketService = basketService;
        _logger = logger;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Get the user's cart.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DishBasketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBasket() {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to GetBasket.");
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            var basket = await _basketService.GetBasketAsync(userId);
            return Ok(basket);
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while retrieving the basket.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = "An unexpected error occurred while retrieving the basket."
            });
        }
    }

    /// <summary>
    /// Add a dish to the cart.
    /// </summary>
    [HttpPost("dish/{dishId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddDishToBasket(Guid dishId) {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to AddDishToBasket.");
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            await _basketService.AddDishToBasketAsync(userId, dishId);
            return Ok(new { Status = "Success", Message = "Dish added to basket." });
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "Dish {DishId} not found.", dishId);
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while adding dish {DishId} to basket.", dishId);
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = "An unexpected error occurred while adding the dish to the basket."
            });
        }
    }

    /// <summary>
    /// Decrease the number of dishes in the cart (if increase = true), or remove the dish completely (increase = false).
    /// </summary>
    [HttpDelete("dish/{dishId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveOrDecreaseDish(Guid dishId, [FromQuery] bool increase = false) {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to RemoveOrDecreaseDish.");
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            await _basketService.RemoveOrDecreaseDishAsync(userId, dishId, increase);
            return Ok(new { Status = "Success", Message = "Dish updated in basket." });
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "Dish {DishId} not found in basket for user {UserId}.", dishId);
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while updating dish {DishId} in basket.", dishId);
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = "An unexpected error occurred while updating the dish in the basket."
            });
        }
    }
}