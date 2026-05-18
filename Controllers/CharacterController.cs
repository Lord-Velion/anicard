using AniCard.Exceptions;
using AniCard.Models.DTOs;
using AniCard.Repositories;
using AniCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AniCard.Controllers
{
    /// <summary>
    /// Handles character card operations: listening/searching, uploading,
    /// downloading, editing, and deleting character card.
    /// </summary>
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

        /// <summary>
        /// Uploads a character card (PNG). Validates the file format,
        /// extracts metadata via the KKLoader microservice, stores the file
        /// in object storage, and persists character data to the database.
        /// </summary>
        /// <param name="dto">The upload payload containing the PNG file,
        /// optional description, and optional tags.</param>
        /// <returns>
        /// <see cref="OkResult"/> on success.
        /// <see cref="BadRequestObjectResult"/> if the model is invalid,
        /// the file fails PNG validation, or KKLoader rejects the card.
        /// <see cref="StatusCodeResult"/> 500 on unexpected errors.
        /// </returns>
        /// <response code="200">Upload succeeded.</response>
        /// <response code="400">Invalid request or unprocessable card.</response>
        /// <response code="401">Missing or invalid JWT.</response>
        /// <response code="413">File exceeds the 10 MB size limit.</response>
        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(10_485_760)]
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
                return Ok();
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

        /// <summary>
        /// Searches and lists character cards with filtering, sorting, and pagination.
        /// Returns a paginated set of character summaries matching the given criteria.
        /// </summary>
        /// <param name="queryParams">
        /// Query parameters containing optional filters (name, tags, sex, personality,
        /// username), sort field and direction, and pagination settings.
        /// </param>
        /// <returns>
        /// <see cref="OkObjectResult"/> containing <see cref="List{CharacterGetDto}"/>
        /// with the matching character cards.
        /// </returns>
        /// <response code="200">Characters matching the query returned.</response>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCharacter([FromQuery] CharactersQueryParams queryParams)
        {
            var result = await _characterRepository.GetCharactersAsync(queryParams);
            return Ok(result);
        }

        /// <summary>
        /// Downloads a character card (PNG) by its unique identifier.
        /// Returns the raw PNG file as a download attachment.
        /// </summary>
        /// <param name="id">The character's unique identifier (GUID).</param>
        /// <returns>
        /// <see cref="FileStreamResult"/> with content type <c>image/png</c>
        /// and the original card filename.
        /// <see cref="NotFoundObjectResult"/> if no character exists with the given ID.
        /// <see cref="StatusCodeResult"/> 500 on unexpected errors.
        /// </returns>
        /// <response code="200">Character card PNG file downloaded.</response>
        /// <response code="404">No character found with the specified ID.</response>
        /// <response code="500">Unexpected error occured.</response>
        [HttpGet("download/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadCharacter(string id)
        {
            _logger.LogInformation("DownloadCharacter started for character ID: {CharacterId}", id);
            try
            {
                var (fileStream, fileName) = await _characterService.DownloadCharacterAsync(id);

                _logger.LogInformation("DownloadCharacter succeeded for character ID: {CharacterId}, FileName: {FileName}", id, fileName);
                return File(fileStream, "image/png", fileName);
            }
            catch(KeyNotFoundException)
            {
                _logger.LogWarning("DownloadCharacter failed: Character with ID '{CharacterId}' not found.", id);
                return NotFound($"Character with id '{id}' not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DownloadCharacter failed with unexpected error for character ID: {CharacterId}", id);
                return StatusCode(500, "An error occurred while downloading the character.");
            }         
        }

        /// <summary>
        /// Deletes a character card and its associated file. Only the owner
        /// of the character is allowed to delete it.
        /// </summary>
        /// <param name="id">The character's unique indentifier.</param>
        /// <returns>200 OK on success</returns>
        /// <response code="200">The character was deleted successfully.</response>
        /// <response code="401">The caller is not authenticated.</response>
        /// <response code="404">No character with the given id exists for the
        /// authenticated user.</response>
        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCharacter(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("DeleteCharacter started for character ID: {CharacterId}, user ID: {UserId}", id, userId);

            await _characterService.DeleteCharacterAsync(id, userId);

            return Ok();
        }

        [HttpPatch("patch/{id}")]
        [Authorize]
        public async Task<IActionResult> PatchCharacter(string id, [FromQuery] string? description, [FromQuery] string[]? tags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _characterService.PatchCharacterAsync(userId, id, description, tags);

            return Ok();
        }

    }
}
