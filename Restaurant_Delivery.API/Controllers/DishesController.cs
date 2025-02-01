using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Delivery.API.BusinessLogicLayer.Services;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

[Authorize]
[ApiController]
[Route("api/dish")]
public class DishController : ControllerBase {
    private readonly IDishService _dishService;
    private readonly ILogger<DishController> _logger;
    private readonly ITokenService _tokenService;

    public DishController(
        IDishService dishService,
        ILogger<DishController> logger,
        ITokenService tokenService) {
        _dishService = dishService;
        _logger = logger;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Get a list of dishes (menu).
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(DishPagedListDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDishes(
        [FromQuery] List<DishCategory> categories,
        [FromQuery] bool? vegetarian = false,
        [FromQuery] DishSorting? sorting = null,
        [FromQuery] int page = 1) {
        try {
            var pagedDishes = await _dishService.GetDishesAsync(categories, vegetarian, sorting, page);
            return Ok(pagedDishes);
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while retrieving dishes.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get information about a specific dish.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DishDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDish(Guid id) {
        try {
            var dish = await _dishService.GetDishAsync(id);
            return Ok(dish);
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "Dish {DishId} not found.", id);
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while retrieving dish {DishId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Checks if the user is able to set a rating for the dish.
    /// </summary>
    [Authorize]
    [HttpGet("{id}/rating/check")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CanRateDish(Guid id) {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to check rating eligibility for dish {DishId}.", id);
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            bool canRate = await _dishService.CanRateDishAsync(id, userId);
            return Ok(canRate);
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "Dish {DishId} not found.", id);
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while checking if the user can rate dish {DishId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Set a rating for a dish.
    /// </summary>
    [Authorize]
    [HttpPost("{id}/rating")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RateDish(Guid id, [FromQuery] double ratingScore) {
        try {
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty) {
                _logger.LogWarning("Unauthorized access attempt to rate dish {DishId}.", id);
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "User is not authenticated or token is revoked."
                });
            }

            await _dishService.RateDishAsync(id, userId, ratingScore);
            return Ok(new { Status = "Success", Message = "Rating updated successfully." });
        } catch (ArgumentException ex) {
            _logger.LogWarning(ex, "Invalid rating score provided for dish {DishId}.", id);
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "Dish {DishId} not found.", id);
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while updating rating for dish {DishId}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }
}