using System;

namespace AniCard.Models.Entities
{
    public class Character
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int Sex { get; set; }
        public int Personality { get; set; }
        public required string ObjectKeyId { get; set; }
        public int Downloads { get; set; } = 0;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public required string UserId { get; set; }
        public User? User { get; set; }
    }
}
