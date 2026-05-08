using AniCard.Models.DTOs;
using AniCard.Models.Entities;

namespace AniCard.Repositories
{
    public interface ICharacterRepository
    {
        Task<Character> UploadCharacterAsync(CharacterMetadataResult metadata, string objectKey, string description, string[] tags, string userId);
    }
}
