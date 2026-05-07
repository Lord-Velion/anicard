using AniCard.Models.DTOs;
using AniCard.Models.Repositories;

namespace AniCard.Models.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly IKKLoaderService _kkLoaderService;
        private readonly ICharacterFileRepository _fileRepository;

        public CharacterService(IKKLoaderService kkLoaderService, ICharacterFileRepository fileRepository)
        {
            _kkLoaderService = kkLoaderService;
            _fileRepository = fileRepository;
        }

        public async Task UploadCharacterAsync(CharacterUploadDto dto)
        {
            await _kkLoaderService.GetCharacterMetadataAsync(dto.File);
            await _fileRepository.UploadCharacterAsync(dto.File);
        }
    }
}
