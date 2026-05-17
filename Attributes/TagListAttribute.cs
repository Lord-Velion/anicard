using System.ComponentModel.DataAnnotations;

namespace AniCard.Attributes;
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TagListAttribute : ValidationAttribute
    {
        public int MaxTags { get; set; } = 10;
        public int MaxTagLength { get; set; } = 50;

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            if (value is string[] tags)
            {
                if (tags.Length > MaxTags)
                    return new ValidationResult($"Maximum {MaxTags} tags allowed.");

                for (int i = 0; i < tags.Length; i++)
                {
                    if (tags[i]?.Length > MaxTagLength)
                        return new ValidationResult($"Tag '{tags[i]}' exceeds maximum " +
                            $"length of {MaxTagLength} characters.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
