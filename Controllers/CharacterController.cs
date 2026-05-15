using AniCard.Exceptions;
using AniCard.Models.DTOs;
using AniCard.Repositories;
using AniCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AniCard.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly CharacterService _characterService;
        private readonly PngValidatorService _pngValidatorService;
        private readonly CharacterRepository _characterRepository;
        private readonly ILogger<CharacterController> _logger;

        public CharacterController(CharacterService characterService, PngValidatorService pngValidatorService, 
            CharacterRepository characterRepository, ILogger<CharacterController> logger)
        {
            _characterService = characterService;
            _pngValidatorService = pngValidatorService;
            _characterRepository = characterRepository;
            _logger = logger;
        }

        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCharacter([FromForm] CharacterUploadDto dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UploadCharacter: invalid model state for file {FileName}", dto.File?.FileName);
                return BadRequest(ModelState);
            }

            var validationResult = _pngValidatorService.ValidateCharacterFile(dto.File);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("UploadCharacter: validation failed for file {FileName}: {ErrorMessage}", dto.File?.FileName, validationResult.ErrorMessage);
                return BadRequest(validationResult.ErrorMessage);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? throw new InvalidOperationException("User ID not found in token.");
                _logger.LogInformation("UploadCharacter started for user {UserId} with file {FileName} ({FileSize} bytes)", userId, dto.File?.FileName, dto.File?.Length);
                await _characterService.UploadCharacterAsync(dto, userId);

                _logger.LogInformation("UploadCharacter succeeded for user {UserId} with file {FileName}", userId, dto.File?.FileName);
                return Ok(new
                {
                    message = "Upload successful",
                    description = dto.Description,
                    tags = dto.Tags,
                    fileName = dto.File.FileName
                });
            }
            catch (InvalidCharacterException ex)
            {
                _logger.LogWarning(ex, "UploadCharacter failed validation in KKLoader for file {FileName}", dto.File?.FileName);
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                _logger.LogError("UploadCharacter failed with unexpected error for file {FileName}", dto.File?.FileName);
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCharacter([FromQuery] CharactersQueryParams queryParams)
        {
            var result = await _characterRepository.GetCharactersAsync(queryParams);
            return Ok(result);
        }
    }
}
