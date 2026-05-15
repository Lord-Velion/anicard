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

        public async Task<List<CharacterGetDto>> GetCharactersAsync(CharactersQueryParams queryParams)
        {
            var query = _context.Characters.AsQueryable();

            // Filter by name (case-insensitive substring match)
            if (!string.IsNullOrWhiteSpace(queryParams.Filter.Name))
            {
                var name = queryParams.Filter.Name.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(name));
            }

            // Filter by tags (must have ALL specified tags)
            if (queryParams.Filter.Tags is { Count: > 0 })
            {
                foreach (var tag in queryParams.Filter.Tags)
                {
                    var currentTag = tag;
                    query = query.Where(c => c.Tags.Any(t => t.Name == currentTag));
                }
            }

            // Filter by sex
            if (queryParams.Filter.Sex.HasValue)
                query = query.Where(c => c.Sex == queryParams.Filter.Sex.Value);

            // Filter by personality
            if (queryParams.Filter.Personality.HasValue)
                query = query.Where(c => c.Personality == queryParams.Filter.Personality.Value);

            // Filter by creator username (case-insensitive substring match)
            if (!string.IsNullOrWhiteSpace(queryParams.Filter.UserName))
            {
                var userName = queryParams.Filter.UserName.ToLower();
                query = query.Where(c => c.User != null && c.User.UserName.ToLower().Contains(userName));
            }

            // Sort by Downloads or Date, ascending or descending (default: Downloads DESC)
            query = (queryParams.OrderBy, queryParams.Sort) switch
            {
                (OrderByField.Downloads, SortOrder.Asc) => query.OrderBy(c => c.Downloads),
                (OrderByField.Downloads, SortOrder.Desc) => query.OrderByDescending(c => c.Downloads),
                (OrderByField.Date, SortOrder.Asc) => query.OrderBy(c => c.UploadedAt),
                (OrderByField.Date, SortOrder.Desc) => query.OrderByDescending(c => c.UploadedAt),
                _ => query.OrderByDescending(c => c.Downloads)
            };

            // Apply pagination
            query = query
                .Skip((queryParams.Pagination.PageNumber - 1) * queryParams.Pagination.PageSize)
                .Take(queryParams.Pagination.PageSize);

            // Project to DTO
            var characters = await query
                .Select(c => new CharacterGetDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Sex = c.Sex,
                    Personality = c.Personality,
                    Downloads = c.Downloads,
                    UploadedAt = c.UploadedAt,
                    TagNames = c.Tags.Select(t => t.Name).ToList(),
                    UserName = c.User != null ? c.User.UserName : null
                }).ToListAsync();

            return characters;
        }
    }
}
