using System.Text.Json.Serialization;

namespace AniCard.Models.DTOs
{
    public class CharacterMetadataResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("sex")]
        public int Sex { get; set; }

        [JsonPropertyName("personality")]
        public int Personality { get; set; }
    }
}
