using AniCard.Models.DTOs;

namespace AniCard.Services
{
    public interface IPngValidatorService
    {
        ValidationResult ValidateCharacter(IFormFile file);
    }
}
