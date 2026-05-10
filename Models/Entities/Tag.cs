using System;
using System.Collections.Generic;

namespace AniCard.Models.Entities
{
    public class Tag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Name { get; set; }
        public ICollection<Character> Characters { get; set; } = new List<Character>();
    }
}
