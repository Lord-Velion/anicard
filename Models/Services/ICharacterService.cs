using AniCard.Models.DTOs;

namespace AniCard.Models.Services
{
    public interface ICharacterService
    {
        Task UploadCharacterAsync(CharacterUploadDto dto);
    }
}
