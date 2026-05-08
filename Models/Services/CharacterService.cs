using AniCard.Models.DTOs;
using AniCard.Models.Repositories;

namespace AniCard.Models.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly IKKLoaderService _kkLoaderService;
        private readonly ICharacterFileRepository _fileRepository;
        private readonly ICharacterRepository _characterRepository;

        public CharacterService(IKKLoaderService kkLoaderService, ICharacterFileRepository fileRepository, ICharacterRepository characterRepository)
        {
            _kkLoaderService = kkLoaderService;
            _fileRepository = fileRepository;
            _characterRepository = characterRepository;
        }

        public async Task UploadCharacterAsync(CharacterUploadDto dto, string userId)
        {
            var metadata = await _kkLoaderService.GetCharacterMetadataAsync(dto.File);
            var objectKey = await _fileRepository.UploadCharacterAsync(dto.File);
            await _characterRepository.UploadCharacterAsync(metadata, objectKey, dto.Description, userId);
        }
    }
}
