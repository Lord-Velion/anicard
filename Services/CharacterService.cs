using AniCard.Models.DTOs;
using AniCard.Repositories;
using Microsoft.Extensions.Logging;
using AniCard.Models.Entities;

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

        public async Task<(Stream FileStream, string CardName)> DownloadCharacterAsync(string id)
        {
            _logger.LogInformation("Downloading character with ID {CharacterId}", id);

            string? objectKey = await _characterRepository.GetObjectKeyAsync(id);
            _logger.LogDebug("Retrieved object key for character {CharacterId}: {ObjectKey}", id, objectKey ?? "null");

            if (objectKey is null)
            {
                _logger.LogWarning("Character with ID {CharacterId} not found in repository", id);
                throw new KeyNotFoundException($"Character with id '{id}' not found.");
            }

            _logger.LogInformation("Fetching file for character {CharacterId} using object key {ObjectKey}", id, objectKey);
            var result = await _fileRepository.DownloadCharacterAsync(objectKey);

            _logger.LogInformation("Successfully downloaded character {CharacterId} with card name {CardName}", id, result.CardName);
            return result;
        }

        public async Task DeleteCharacterAsync(string characterId, string userId)
        {
            var objectKey = await _characterRepository.GetObjectKeyAsync(characterId, userId);

            if (objectKey is null)
            {
                throw new KeyNotFoundException($"Character with character id {characterId} and user id {userId} not found");
            }

            await _characterRepository.DeleteCharacterAsync(characterId, userId);

            await _fileRepository.DeleteCharacterAsync(objectKey);
        }

        public async Task PatchCharacterAsync(string userId, string characterId, string? description, string[]? tags)
        {
            await _characterRepository.PatchCharacterAsync(userId, characterId, description, tags);
        }
    }
}
