using AniCard.Models.DTOs;
using AniCard.Repositories;

namespace AniCard.Services
{
    public class CharacterService
    {
        private readonly KKLoaderService _kkLoaderService;
        private readonly CharacterFileRepository _fileRepository;
        private readonly CharacterRepository _characterRepository;

        public CharacterService(KKLoaderService kkLoaderService, CharacterFileRepository fileRepository, CharacterRepository characterRepository)
        {
            _kkLoaderService = kkLoaderService;
            _fileRepository = fileRepository;
            _characterRepository = characterRepository;
        }

        public async Task UploadCharacterAsync(CharacterUploadDto dto, string userId)
        {
            var metadata = await _kkLoaderService.GetCharacterMetadataAsync(dto.File);
            var objectKey = await _fileRepository.UploadCharacterAsync(dto.File);
            await _characterRepository.UploadCharacterAsync(metadata, objectKey, dto.Description, dto.Tags, userId);
        }
    }
}
