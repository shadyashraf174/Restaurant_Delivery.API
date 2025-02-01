using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant_Delivery.API.BusinessLogicLayer.Services;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

[Authorize]
[ApiController]
[Route("api/account")]
[Produces("application/json")]
public class UsersController : ControllerBase {
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly ITokenService _tokenService;

    public UsersController(
        IUserService userService,
        ILogger<UsersController> logger,
        ITokenService tokenService) {
        _userService = userService;
        _logger = logger;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] UserRegisterModel request) {
        try {
            var tokenResponse = await _userService.RegisterAsync(request);
            return Ok(tokenResponse);
        } catch (ArgumentException ex) {
            _logger.LogWarning(ex, "Invalid request data for registration.");
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (InvalidOperationException ex) {
            _logger.LogWarning(ex, "Email is already registered.");
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while registering user.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Log in to the system.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginCredentials request) {
        try {
            var tokenResponse = await _userService.LoginAsync(request);
            return Ok(tokenResponse);
        } catch (ArgumentException ex) {
            _logger.LogWarning(ex, "Invalid request data for login.");
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (InvalidOperationException ex) {
            _logger.LogWarning(ex, "Invalid email or password.");
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while logging in user.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Log out the system user.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout() {
        try {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            await _userService.LogoutAsync(token);
            return Ok(new { Status = "Success", Message = "Logged out successfully." });
        } catch (UnauthorizedAccessException ex) {
            _logger.LogWarning(ex, "Invalid or expired token.");
            return Unauthorized(new Response {
                Status = "Unauthorized",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while logging out user.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get the user's profile.
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile() {
        try {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty || await _tokenService.IsTokenRevokedAsync(token)) {
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "Invalid token."
                });
            }

            var userDto = await _userService.GetProfileAsync(userId);
            return Ok(userDto);
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "User not found.");
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while fetching user profile.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Edit the user's profile.
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EditProfile([FromBody] UserEditModel request) {
        try {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userId = await _tokenService.GetCurrentUserIdAsync(Request, User);
            if (userId == Guid.Empty || await _tokenService.IsTokenRevokedAsync(token)) {
                return Unauthorized(new Response {
                    Status = "Unauthorized",
                    Message = "Invalid token."
                });
            }

            await _userService.UpdateProfileAsync(userId, request);
            return Ok(new { Status = "Success", Message = "Profile updated successfully." });
        } catch (ArgumentException ex) {
            _logger.LogWarning(ex, "Invalid request data for updating profile.");
            return BadRequest(new Response {
                Status = "Bad Request",
                Message = ex.Message
            });
        } catch (KeyNotFoundException ex) {
            _logger.LogWarning(ex, "User not found.");
            return NotFound(new Response {
                Status = "Not Found",
                Message = ex.Message
            });
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while updating user profile.");
            return StatusCode(StatusCodes.Status500InternalServerError, new Response {
                Status = "Error",
                Message = ex.Message
            });
        }
    }
}