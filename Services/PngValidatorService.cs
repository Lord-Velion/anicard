using AniCard.Models.DTOs;

namespace AniCard.Services
{
    public class PngValidatorService
    {
        public ValidationResult ValidateCharacterFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ValidationResult.Failure("File is required.");

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileExtension != ".png")
                return ValidationResult.Failure("Only PNG files are allowed.");

            // TODO: Implement actual character card validation logic
            return ValidationResult.Success();
        }
    }
}
