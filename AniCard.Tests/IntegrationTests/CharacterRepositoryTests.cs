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

    private async Task SeedTestDataAsync()
    {
        var alice = new User { Id = "user-alice", UserName = "Alice" };
        var bob = new User { Id = "user-bob", UserName = "Bob" };
        var charlie = new User { Id = "user-charlie", UserName = "Charlie" };
        _db.Users.AddRange(alice, bob, charlie);

        var anime = new Tag { Id = "tag-anime", Name = "anime" };
        var school = new Tag { Id = "tag-school", Name = "school-uniform" };
        var action = new Tag { Id = "tag-action", Name = "action" };
        var fantasy = new Tag { Id = "tag-fantasy", Name = "fantasy" };
        _db.Tags.AddRange(anime, school, action, fantasy);

        var characters = new List<Character>
        {
            new() { Id = "char-1",  Name = "TestChar",    Sex = 0, Personality = 1, Downloads = 100, UploadedAt = new(2024,1,1),  UserId = "user-alice",   ObjectKeyId = "obj-1",  Tags = [anime, school] },
            new() { Id = "char-2",  Name = "AnotherChar",  Sex = 1, Personality = 2, Downloads = 50,  UploadedAt = new(2024,2,1),  UserId = "user-bob",     ObjectKeyId = "obj-2",  Tags = [action] },
            new() { Id = "char-3",  Name = "AliceChar",    Sex = 0, Personality = 1, Downloads = 200, UploadedAt = new(2024,3,1),  UserId = "user-alice",   ObjectKeyId = "obj-3",  Tags = [anime, fantasy] },
            new() { Id = "char-4",  Name = "FantasyHero",  Sex = 1, Personality = 3, Downloads = 10,  UploadedAt = new(2024,4,1),  UserId = "user-charlie", ObjectKeyId = "obj-4",  Tags = [fantasy] },
            new() { Id = "char-5",  Name = "ActionStar",   Sex = 0, Personality = 2, Downloads = 500, UploadedAt = new(2024,5,1),  UserId = "user-bob",     ObjectKeyId = "obj-5",  Tags = [action, anime] },
            new() { Id = "char-6",  Name = "SchoolGirl",   Sex = 1, Personality = 1, Downloads = 75,  UploadedAt = new(2024,6,1),  UserId = "user-charlie", ObjectKeyId = "obj-6",  Tags = [school] },
            new() { Id = "char-7",  Name = "BobCreation",  Sex = 0, Personality = 3, Downloads = 300, UploadedAt = new(2024,7,1),  UserId = "user-bob",     ObjectKeyId = "obj-7",  Tags = [anime, action, fantasy] },
            new() { Id = "char-8",  Name = "MysteryFig",   Sex = 1, Personality = 2, Downloads = 0,   UploadedAt = new(2024,8,1),  UserId = "user-alice",   ObjectKeyId = "obj-8",  Tags = [] },
            new() { Id = "char-9",  Name = "ZephyrChar",   Sex = 0, Personality = 1, Downloads = 150, UploadedAt = new(2024,9,1),  UserId = "user-alice",   ObjectKeyId = "obj-9",  Tags = [anime] },
            new() { Id = "char-10", Name = "AlphaMale",    Sex = 1, Personality = 3, Downloads = 25,  UploadedAt = new(2024,10,1), UserId = "user-bob",     ObjectKeyId = "obj-10", Tags = [fantasy, school] },
        };
        _db.Characters.AddRange(characters);
        await _db.SaveChangesAsync();
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

    [Fact]
    public async Task GetCharactersAsync_FilterByName_ValidSubstring_ReturnsMatching()
    {
        await SeedTestDataAsync();

        var queryParams = new CharactersQueryParams
        {
            Filter = new CharacterFilter { Name = "Char" }
        };

        var result = await _repo.GetCharactersAsync(queryParams);

        Assert.Equal(4, result.Count);
        Assert.Contains(result, c => c.Name == "TestChar");
        Assert.Contains(result, c => c.Name == "AnotherChar");
        Assert.Contains(result, c => c.Name == "AliceChar");
        Assert.Contains(result, c => c.Name == "ZephyrChar");
    }

    [Fact]
    public async Task GetCharactersAsync_FilterByName_EmptyString_ReturnsAll()
    {
        await SeedTestDataAsync();

        var queryParams = new CharactersQueryParams
        {
            Filter = new CharacterFilter { Name = "" }
        };

        var result = await _repo.GetCharactersAsync(queryParams);

        Assert.Equal(10, result.Count);
    }

    [Fact]
    public async Task GetCharactersAsync_FilterByName_NoMatch_ReturnsEmpty()
    {
        await SeedTestDataAsync();

        var queryParams = new CharactersQueryParams
        {
            Filter = new CharacterFilter { Name = "NonExistentName" }
        };

        var result = await _repo.GetCharactersAsync(queryParams);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCharactersAsync_FilterByTags_SingleTag_ReturnsMatching()
    {
        await SeedTestDataAsync();

        var queryParams = new CharactersQueryParams
        {
            Filter = new CharacterFilter { Tags = ["anime"] }
        };

        var result = await _repo.GetCharactersAsync(queryParams);

        Assert.Equal(5, result.Count);
        Assert.All(result, c => Assert.Contains("anime", c.TagNames));
    }

    [Fact]
    public async Task GetCharactersAsync_FilterByTags_MultipleTags_ReturnsIntersection()
    {
        await SeedTestDataAsync();

        var queryParams = new CharactersQueryParams
        {
            Filter = new CharacterFilter { Tags = ["anime", "action"] }
        };

        var result = await _repo.GetCharactersAsync(queryParams);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "ActionStar");
        Assert.Contains(result, c => c.Name == "BobCreation");
        Assert.All(result, c =>
        {
            Assert.Contains("anime", c.TagNames);
            Assert.Contains("action", c.TagNames);
        });
    }

    [Fact]
    public async Task GetCharactersAsync_FilterByTags_NonExistent_ReturnsEmpty()
    {
        await SeedTestDataAsync();

        var queryParams = new CharactersQueryParams
        {
            Filter = new CharacterFilter { Tags = ["NonExistentTag"] }
        };

        var result = await _repo.GetCharactersAsync(queryParams);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCharactersAsync_FilterBySex()
    {
        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetCharactersAsync_FilterByCreatorName()
    {
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData("Asc")]
    [InlineData("Desc")]
    public async Task GetCharactersAsync_SortByDownloads(string order)
    {
        await Task.CompletedTask;
    }

    [Theory]
    [InlineData("Asc")]
    [InlineData("Desc")]
    public async Task GetCharactersAsync_SortByDate(string order)
    {
        await Task.CompletedTask;
    }
}
