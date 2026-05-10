using AniCard.Exceptions;
using AniCard.Models.DTOs;
using AniCard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AniCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly CharacterService _characterService;
        private readonly PngValidatorService _pngValidatorService;

        public CharacterController(CharacterService characterService, PngValidatorService pngValidatorService)
        {
            _characterService = characterService;
            _pngValidatorService = pngValidatorService;
        }

        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCharacter([FromForm] CharacterUploadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var validationResult = _pngValidatorService.ValidateCharacter(dto.File);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? throw new InvalidOperationException("User ID not found in token.");
                await _characterService.UploadCharacterAsync(dto, userId);

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
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred. Please try again later.");
            }
        }
    }
}
