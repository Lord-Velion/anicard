using AniCard.Data;
using AniCard.Models.DTOs;
using AniCard.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AniCard.Repositories
{
    public class CharacterRepository
    {
        private readonly AppDbContext _context;

        public CharacterRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Character> UploadCharacterAsync(CharacterMetadataResult metadata, string objectKey, string description, string[] tags, string userId)
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

            if (tags != null && tags.Length > 0)
            {
                var uniqueTags = tags.Select(t => t.Trim())
                                    .Where(t => !string.IsNullOrEmpty(t))
                                    .Distinct();

                foreach (var tagName in uniqueTags)
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (existingTag == null)
                    {
                        existingTag = new Tag { Name = tagName };
                        _context.Tags.Add(existingTag);
                    }
                    character.Tags.Add(existingTag);
                }
            }

            _context.Characters.Add(character);
            await _context.SaveChangesAsync();
            return character;
        }
    }
}
