namespace AniCard.Repositories
{
    public class CharacterFileRepository : ICharacterFileRepository
    {
        public Task<string> UploadCharacterAsync(IFormFile file)
        {
            var objectKey = $"{Guid.NewGuid()}-{file.FileName}";
            return Task.FromResult(objectKey);
        }
    }
}
