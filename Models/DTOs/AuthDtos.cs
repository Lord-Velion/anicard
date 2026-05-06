namespace AniCard.Models.DTOs
{
    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
    }
}
