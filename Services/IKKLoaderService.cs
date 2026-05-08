using AniCard.Models.DTOs;

namespace AniCard.Services
{
    public interface IKKLoaderService
    {
        Task<CharacterMetadataResult> GetCharacterMetadataAsync(IFormFile file);
    }
}
