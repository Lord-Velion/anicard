namespace AniCard.Models.DTOs
{
    public class CharacterUploadDto
    {
        public string? Description { get; set; }
        public string[]? Tags { get; set; }
        public required IFormFile File { get; set; }
    }
}
