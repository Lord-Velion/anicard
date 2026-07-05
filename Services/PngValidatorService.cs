using AniCard.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace AniCard.Services
{
    /// <summary>
    /// Validates uploaded character card files. Currently checks for
    /// non-empty PNG files with the correct extension.
    /// </summary>
    public class PngValidatorService
    {
        private readonly ILogger<PngValidatorService> _logger;

        public PngValidatorService(ILogger<PngValidatorService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Validates that the uploaded file is a non-empty PNG.
        /// </summary>
        /// <param name="file">The uploaded file from the request.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating success or containing an error message.</returns>
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
