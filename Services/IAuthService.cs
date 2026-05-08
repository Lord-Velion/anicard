using AniCard.Models.DTOs;

namespace AniCard.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);
        Task<AuthResult> LoginAsync(LoginRequest request);
        Task LogoutAsync();
    }
}
