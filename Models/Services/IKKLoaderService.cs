using AniCard.Models.DTOs;

namespace AniCard.Models.Services
{
    public interface IKKLoaderService
    {
        Task<CharacterMetadataResult> GetCharacterMetadataAsync(IFormFile file);
    }
}
