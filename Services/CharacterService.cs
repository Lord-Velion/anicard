using AniCard.Models.DTOs;
using AniCard.Repositories;
using Microsoft.Extensions.Logging;

namespace AniCard.Services
{
    public class CharacterService
    {
        private readonly KKLoaderService _kkLoaderService;
        private readonly CharacterFileRepository _fileRepository;
        private readonly CharacterRepository _characterRepository;
        private readonly ILogger<CharacterService> _logger;

        public CharacterService(KKLoaderService kkLoaderService, CharacterFileRepository fileRepository, CharacterRepository characterRepository, ILogger<CharacterService> logger)
        {
            _kkLoaderService = kkLoaderService;
            _fileRepository = fileRepository;
            _characterRepository = characterRepository;
            _logger = logger;
        }

        public async Task UploadCharacterAsync(CharacterUploadDto dto, string userId)
        {
            _logger.LogInformation("Character upload flow started for user {UserId} with file {FileName}", userId, dto.File?.FileName);

            var metadata = await _kkLoaderService.GetCharacterMetadataAsync(dto.File);
            _logger.LogInformation("Character metadata fetched for user {UserId} with file {FileName}", userId, dto.File?.FileName);

            var objectKey = await _fileRepository.UploadCharacterAsync(dto.File);
            _logger.LogInformation("Character file uploaded for user {UserId} with object key {ObjectKey}", userId, objectKey);

            var character = await _characterRepository.UploadCharacterAsync(metadata, objectKey, dto.Description, dto.Tags, userId);
            _logger.LogInformation("Character saved to database for user {UserId} with character id {CharacterId}", userId, character.Id);
        }
    }
}
