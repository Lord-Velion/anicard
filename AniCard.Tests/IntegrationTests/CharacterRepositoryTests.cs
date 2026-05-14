using AniCard.Data;
using AniCard.Repositories;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.Logging;
using AniCard.Models.DTOs;
using AniCard.Models.Entities;

namespace AniCard.Tests.IntegrationTests;

public class CharacterRepositoryTests
{
    private readonly AppDbContext _db;
    private readonly CharacterRepository _repo;

    public CharacterRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _repo = new CharacterRepository(_db, Mock.Of<ILogger<CharacterRepository>>());
    }

    [Fact]
    public async Task UploadCharacterAsync_CreatesCharacterWithTags()
    {
        // Arrange
        var metadata = new CharacterMetadataResult
        {
            Name = "Test Char",
            Sex = 0,
            Personality = 1
        };
        var tags = new[] { "anime", "school-uniform" };

        // Act
        var character = await _repo.UploadCharacterAsync(
            metadata, "obj-key", "A test character", tags, "user-1");

        // Assert
        Assert.Equal("Test Char", character.Name);
        Assert.Equal(2, character.Tags.Count);
        Assert.Contains(character.Tags, t => t.Name == "anime");
    }

    [Fact]
    public async Task UploadCharacterAsync_TagsIntersect_TagTableIsUnique()
    {
        // Arrange

        var meta1 = new CharacterMetadataResult { Name = "Char A", Sex = 0, Personality = 1 };
        var meta2 = new CharacterMetadataResult { Name = "Char B", Sex = 1, Personality = 2 };

        // Act
        await _repo.UploadCharacterAsync(meta1, "obj-key-1", null, 
            ["anime", "school-uniform"], "user-1");
        await _repo.UploadCharacterAsync(meta2, "obj-key-2", null,
            ["anime", "action"], "user-1");

        // Assert
        var allTags = await _db.Tags.ToListAsync();
        Assert.Equal(3, allTags.Count);
        Assert.Contains(allTags, t => t.Name == "anime");
        Assert.Contains(allTags, t => t.Name == "school-uniform");
        Assert.Contains(allTags, t => t.Name == "action");

        // Verify both characters reference the same "anime" tag instance
        var animeTag = allTags.Single(t => t.Name == "anime");
        var charA = await _db.Characters.Include(c => c.Tags)
            .FirstAsync(c => c.Name == "Char A");
        var charB = await _db.Characters.Include(c => c.Tags)
            .FirstAsync(c => c.Name == "Char B");
        Assert.Contains(animeTag, charA.Tags);
        Assert.Contains(animeTag, charB.Tags);
    }
}
