using AniCard.Data;
using AniCard.Models.DTOs;
using AniCard.Models.Entities;

namespace AniCard.Models.Repositories
{
    public class CharacterRepository : ICharacterRepository
    {
        private readonly AppDbContext _context;

        public CharacterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Character> UploadCharacterAsync(CharacterMetadataResult metadata, string objectKey, string description, string userId)
        {
            var character = new Character
            {
                Name = metadata.Name,
                Sex = metadata.Sex,
                Personality = metadata.Personality,
                Description = description,
                ObjectKeyId = objectKey,
                UserId = userId
            };

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();
            return character;
        }
    }
}
