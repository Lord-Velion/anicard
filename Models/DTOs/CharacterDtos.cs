using System.ComponentModel.DataAnnotations;
using AniCard.Attributes;

namespace AniCard.Models.DTOs
{
    public class CharacterUploadDto
    {
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [TagList(MaxTags = 10, MaxTagLength = 50, ErrorMessage = "Invalid tags.")]
        public string[]? Tags { get; set; }

        public required IFormFile File { get; set; }
    }

    public class CharacterGetDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Sex { get; set; }
        public int Personality { get; set; }
        public int Downloads { get; set; }
        public DateTime UploadedAt { get; set; }
        public List<string> TagNames { get; set; }
        public string UserName { get; set; }
    }
}
