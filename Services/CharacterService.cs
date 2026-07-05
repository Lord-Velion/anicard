using AniCard.Models.DTOs;
using AniCard.Repositories;
using Microsoft.Extensions.Logging;
using AniCard.Models.Entities;

namespace AniCard.Services
{
    /// <summary>
    /// Orchestrates character card business logic: upload, download, delete, and patch operations,
    /// coordinating between the KKLoader microservice, file storage, and database repositories.
    /// </summary>
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

        /// <summary>
        /// Orchestrates the full upload pipeline: extracts card metadata from
        /// the KKLoader microservice, uploads the raw file to MinIO, and
        /// persists the character record together with tags to the database.
        /// </summary>
        /// <param name="dto">The upload DTO containing the file, description,
        /// and tags.</param>
        /// <param name="userId">The authenticated user's identifier.</param>
        /// <exception cref="InvalidCharacterException">
        /// Thrown when KKLoader rejects the PNG (e.g. not a valid character card).
        /// </exception>
        public async Task UploadCharacterAsync(CharacterUploadDto dto, string userId)
        {
            _logger.LogInformation("Character upload flow started for user {UserId} with file {FileName}", userId, dto.File?.FileName);

            CharacterMetadataResult metadata = await _kkLoaderService.GetCharacterMetadataAsync(dto.File);
            _logger.LogInformation("Character metadata fetched for user {UserId} with file {FileName}", userId, dto.File?.FileName);

            string objectKey = await _fileRepository.UploadCharacterAsync(dto.File);
            _logger.LogInformation("Character file uploaded for user {UserId} with object key {ObjectKey}", userId, objectKey);

            Character character = await _characterRepository.UploadCharacterAsync(metadata, objectKey, dto.Description, dto.Tags, userId);
            _logger.LogInformation("Character saved to database for user {UserId} with character id {CharacterId}", userId, character.Id);
        }

        /// <summary>
        /// Retrieves a character card file by resolving its object key from the
        /// database and downloading the raw PNG from object storage.
        /// </summary>
        /// <param name="id">The character's unique identifier (GUID).</param>
        /// <returns>
        /// A tuple containing the file stream and the card file name.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no character exists with the given ID>
        /// </exception>
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

        /// <summary>
        /// Orchestrates character deletion: verifies ownership via the
        /// character's object key, removes the database record, then
        /// deletes the associated file from object storage.
        /// </summary>
        /// <param name="characterId">The character's unique identifier.</param>
        /// <param name="userId">The authenticated user's identifier used for
        /// ownership verification.</param>
        /// <exception cref="KeyNotFoundException">Thrown when no character
        /// matching both <paramref name="characterId"/> and
        /// <paramref name="userId"/> is found.</exception>
        public async Task DeleteCharacterAsync(string characterId, string userId)
        {
            _logger.LogInformation("DeleteCharacter flow started for character ID: {CharacterId}, user ID: {UserId}", characterId, userId);

            var objectKey = await _characterRepository.GetObjectKeyAsync(characterId, userId);

            if (objectKey is null)
            {
                _logger.LogWarning("DeleteCharacter failed: character ID {CharacterId} not found for user {UserId}", characterId, userId);
                throw new KeyNotFoundException($"Character with character id {characterId} and user id {userId} not found");
            }

            _logger.LogDebug("DeleteCharacter found object key {ObjectKey} for character ID {CharacterId}", objectKey, characterId);

            await _characterRepository.DeleteCharacterAsync(characterId, userId);
            _logger.LogDebug("DeleteCharacter removed database record for character ID {CharacterId}", characterId);

            await _fileRepository.DeleteCharacterAsync(objectKey);
            _logger.LogInformation("DeleteCharacter succeeded for character ID {CharacterId}", characterId);
        }

        /// <summary>
        /// Delegates to the repository to partially update a character's
        /// description and tags.
        /// </summary>
        /// <param name="userId">The authenticated user's identifier used for
        /// ownership verification.</param>
        /// <param name="characterId">The character's unique identifier.</param>
        /// <param name="description">An optional new description.</param>
        /// <param name="tags">An optional array of tag names replacing
        /// existing tags.</param>
        public async Task PatchCharacterAsync(string userId, string characterId, string? description, string[]? tags)
        {
            _logger.LogInformation("PatchCharacter flow started for character ID: {CharacterId}, user ID: {UserId}", characterId, userId);

            await _characterRepository.PatchCharacterAsync(userId, characterId, description, tags);

            _logger.LogInformation("PatchCharacter succeeded for character ID: {CharacterId}", characterId);
        }
    }
}
