using AniCard.Services;
using Castle.Core.Logging;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace AniCard.Tests.UnitTests.Services;

public class PngValidatorServiceTests
{
    private readonly PngValidatorService _sut;
    private readonly Mock<ILogger<PngValidatorService>> _loggerMock = new();

    public PngValidatorServiceTests()
    {
        _sut = new PngValidatorService(_loggerMock.Object);
    }

    [Fact]
    public void ValidateCharacterFile_NullFile_ReturnsFailure()
    {
        var result = _sut.ValidateCharacterFile(null);

        Assert.False(result.IsValid);
        Assert.Equal("File is required.", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCharacterFile_EmptyFile_ReturnsFailure()
    {
        var file = new FormFile(null, 0, 0, "file", "test.png");

        var result = _sut.ValidateCharacterFile(file);

        Assert.False(result.IsValid);
        Assert.Equal("File is required.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("test.jpg")]
    [InlineData("test.jpeg")]
    [InlineData("test")]
    [InlineData("test.png.exe")]
    public void ValidateCharacterFile_NonPngExtension_ReturnsFailure(string fileName)
    {
        var file = new FormFile(null, 0, 1, "file", fileName);

        var result = _sut.ValidateCharacterFile(file);

        Assert.False(result.IsValid);
        Assert.Equal("Only PNG files are allowed.", result.ErrorMessage);
    }

    [Fact]
    public void ValidateCharacterFile_ValidPngFile_ReturnsSuccess()
    {
        var stream = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47 }); // PNG magic bytes
        var file = new FormFile(stream, 0, stream.Length, "file", "character.png");

        var result = _sut.ValidateCharacterFile(file);

        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }
}
