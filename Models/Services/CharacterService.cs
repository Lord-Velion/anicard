using AniCard.Models.DTOs;

namespace AniCard.Models.Services
{
    public class CharacterService : ICharacterService
    {
        public Task UploadCharacterAsync(CharacterUploadDto dto)
        {
            return Task.CompletedTask;
        }
    }
}
