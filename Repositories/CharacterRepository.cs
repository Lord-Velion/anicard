using AniCard.Data;
using AniCard.Models.DTOs;
using AniCard.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AniCard.Repositories
{
    public class CharacterRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CharacterRepository> _logger;

        public CharacterRepository(AppDbContext context, ILogger<CharacterRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Character> UploadCharacterAsync(CharacterMetadataResult metadata, string objectKey, string? description, string[]? tags, string userId)
        {
            _logger.LogInformation("Saving character for user {UserId} with object key {ObjectKey}", userId, objectKey);
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
                _logger.LogDebug("Processing {TagCount} tags for user {UserId}", tags.Length, userId);
                 var uniqueTags = tags.SelectMany(t => t.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                    .Distinct(StringComparer.OrdinalIgnoreCase);

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
            _logger.LogInformation("Character saved with id {CharacterId} for user {UserId}", character.Id, userId);
            return character;
        }

        public async Task<List<Character>> GetCharactersAsync(CharactersQueryParams queryParams)
        {
            var characters = await _context.Characters
                .Include(c => c.User)
                .Include(c => c.Tags)
                .ToListAsync();

            return characters;
        }
    }
}
