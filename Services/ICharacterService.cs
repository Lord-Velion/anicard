using AniCard.Models.DTOs;

namespace AniCard.Services
{
    public interface ICharacterService
    {
        Task UploadCharacterAsync(CharacterUploadDto dto, string userId);
    }
}
