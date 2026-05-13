using AniCard.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace AniCard.Services
{
    public class PngValidatorService
    {
        private readonly ILogger<PngValidatorService> _logger;

        public PngValidatorService(ILogger<PngValidatorService> logger)
        {
            _logger = logger;
        }

        public ValidationResult ValidateCharacterFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("PNG validation failed: empty or missing file");
                return ValidationResult.Failure("File is required.");
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (fileExtension != ".png")
            {
                _logger.LogWarning("PNG validation failed: invalid extension {Extension} for file {FileName}", fileExtension, file.FileName);
                return ValidationResult.Failure("Only PNG files are allowed.");
            }

            // TODO: Implement actual character card validation logic
            return ValidationResult.Success();
        }
    }
}
