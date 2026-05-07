using AniCard.Models.DTOs;

namespace AniCard.Models.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly IKKLoaderService _kkLoaderService;

        public CharacterService(IKKLoaderService kkLoaderService)
        {
            _kkLoaderService = kkLoaderService;
        }

        public async Task UploadCharacterAsync(CharacterUploadDto dto)
        {
            await _kkLoaderService.GetCharacterMetadataAsync(dto.File);
        }
    }
}
