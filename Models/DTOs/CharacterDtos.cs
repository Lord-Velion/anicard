namespace AniCard.Models.DTOs
{
    public class CharacterUploadDto
    {
        public required string Description { get; set; }
        public required string[] Tags { get; set; }
        public required IFormFile File { get; set; }
    }
}
