using AniCard.Models.DTOs;
using AniCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AniCard.Controllers
{
    /// <summary>
    /// Handles user authentication: registration, login, logout, and token verification.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration payload containing username, email, and password.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> with the registration result on success.
        /// <see cref="BadRequestObjectResult"/> if the request is invalid or registration fails.
        /// </returns>
        /// <response code="200">Registration succeeded.</response>
        /// <response code="400">Invalid request or registration failed.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Register started for username: {Username}", request.Username);

            try
            {
                var result = await _authService.RegisterAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Register failed for username: {Username}, message: {Message}", request.Username, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Register succeeded for username: {Username}", request.Username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register failed with unexpected error for username: {Username}", request.Username);
                return StatusCode(500, "An unexpected error occurred during registration.");
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="request">The login payload containing username and password.</param>
        /// <returns>
        /// <see cref="OkObjectResult"/> with the authentication result including the JWT token.
        /// <see cref="UnauthorizedObjectResult"/> if the credentials are invalid.
        /// </returns>
        /// <response code="200">Login succeeded, JWT token returned.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login started for username: {Username}", request.Username);

            try
            {
                var result = await _authService.LoginAsync(request);

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Login failed for username: {Username}, message: {Message}", request.Username, result.Message);
                    return Unauthorized(result);
                }

                _logger.LogInformation("Login succeeded for username: {Username}", request.Username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed with unexpected error for username: {Username}", request.Username);
                return StatusCode(500, "An unexpected error occurred during login.");
            }
        }

        /// <summary>
        /// Logs out the current user. Token invalidation is handled client-side.
        /// </summary>
        /// <returns>A message indicating the user should remove their token.</returns>
        /// <response code="200">Logout acknowledged.</response>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Logout started");

            try
            {
                await _authService.LogoutAsync();

                _logger.LogInformation("Logout succeeded");
                return Ok(new { Message = "Logout successful. Remove token from client." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed with unexpected error");
                return StatusCode(500, "An unexpected error occurred during logout.");
            }
        }

        /// <summary>
        /// Verifies the current JWT token and returns the authenticated user's details.
        /// </summary>
        /// <returns>The authenticated user's ID and username.</returns>
        /// <response code="200">Token is valid, user details returned.</response>
        /// <response code="401">Missing or invalid JWT.</response>
        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var username = User.FindFirstValue(ClaimTypes.Name);

            _logger.LogInformation("Protected endpoint accessed by user ID: {UserId}, username: {Username}", userId, username);

            return Ok(new
            {
                Message = "Authentication successful",
                UserId = userId,
                Username = username
            });
        }
    }
}
