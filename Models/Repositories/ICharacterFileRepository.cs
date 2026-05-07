namespace AniCard.Models.Repositories
{
    public interface ICharacterFileRepository
    {
        Task<string> UploadCharacterAsync(IFormFile file);
    }
}
