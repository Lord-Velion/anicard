namespace AniCard.Repositories
{
    public interface ICharacterFileRepository
    {
        Task<string> UploadCharacterAsync(IFormFile file);
    }
}
