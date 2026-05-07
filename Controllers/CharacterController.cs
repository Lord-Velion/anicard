using AniCard.Models.DTOs;
using AniCard.Models.Exceptions;
using AniCard.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AniCard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpPost("upload")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCharacter([FromForm] CharacterUploadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("File is required.");

            var fileExtension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (fileExtension != ".png")
                return BadRequest("Only PNG files are allowed.");

            try
            {
                await _characterService.UploadCharacterAsync(dto);

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
        }
    }
}
