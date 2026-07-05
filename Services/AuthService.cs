using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AniCard.Models.DTOs;
using AniCard.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AniCard.Services
{
    /// <summary>
    /// Handles user authentication: registration, login, logout, and JWT token generation.
    /// </summary>
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<User> userManager, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user with the given credentials.
        /// </summary>
        /// <param name="request">The registration payload containing username, email, and password.</param>
        /// <returns>An <see cref="AuthResult"/> indicating success or failure with a message.</returns>
        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("RegisterAsync started for username: {Username}, email: {Email}", request.Username, request.Email);

            try
            {
                var existingUser = await _userManager.FindByNameAsync(request.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("RegisterAsync failed: username {Username} already exists", request.Username);
                    return new AuthResult { IsSuccess = false, Message = "Username already exists" };
                }

                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingEmail != null)
                {
                    _logger.LogWarning("RegisterAsync failed: email {Email} already exists", request.Email);
                    return new AuthResult { IsSuccess = false, Message = "Email already exists" };
                }

                var user = new User
                {
                    UserName = request.Username,
                    Email = request.Email
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("RegisterAsync failed for username: {Username}, errors: {Errors}", request.Username, errors);
                    return new AuthResult { IsSuccess = false, Message = errors };
                }

                _logger.LogInformation("RegisterAsync succeeded for username: {Username}, user ID: {UserId}", request.Username, user.Id);
                return new AuthResult
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    Message = "Registration successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAsync failed with unexpected error for username: {Username}", request.Username);
                return new AuthResult { IsSuccess = false, Message = "An unexpected error occurred during registration." };
            }
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token on success.
        /// </summary>
        /// <param name="request">The login payload containing username/email and password.</param>
        /// <returns>An <see cref="AuthResult"/> containing the JWT token on success, or a failure message.</returns>
        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("LoginAsync started for username: {Username}", request.Username);

            try
            {
                var user = await _userManager.FindByNameAsync(request.Username)
                           ?? await _userManager.FindByEmailAsync(request.Username);

                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    _logger.LogWarning("LoginAsync failed: invalid credentials for username: {Username}", request.Username);
                    return new AuthResult { IsSuccess = false, Message = "Invalid credentials" };
                }

                var token = GenerateJwtToken(user);

                _logger.LogInformation("LoginAsync succeeded for username: {Username}, user ID: {UserId}", request.Username, user.Id);
                return new AuthResult
                {
                    IsSuccess = true,
                    Token = token,
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoginAsync failed with unexpected error for username: {Username}", request.Username);
                return new AuthResult { IsSuccess = false, Message = "An unexpected error occurred during login." };
            }
        }

        /// <summary>
        /// Logs out the current user. Token invalidation is handled client-side.
        /// </summary>
        /// <returns>A completed task.</returns>
        public Task LogoutAsync()
        {
            _logger.LogInformation("LogoutAsync completed");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The authenticated user entity.</param>
        /// <returns>A signed JWT token string.</returns>
        private string GenerateJwtToken(User user)
        {
            _logger.LogDebug("Generating JWT token for user ID: {UserId}, username: {Username}", user.Id, user.UserName);

            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expireMinutes = double.Parse(jwtSettings["ExpireMinutes"]);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            _logger.LogDebug("JWT token generated for user ID: {UserId}", user.Id);
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
