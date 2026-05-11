using AniCard.Configuration;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace AniCard.Repositories
{
    public class CharacterFileRepository
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _settings;

        public CharacterFileRepository(
            IMinioClient minioClient,
            IOptions<MinioSettings> settings)
        {
            _minioClient = minioClient;
            _settings = settings.Value;
        }

        public async Task<string> UploadCharacterAsync(IFormFile file)
        {
            var objectKey = $"{Guid.NewGuid()}-{file.FileName}";

            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs()
                    .WithBucket(_settings.BucketName));

            if (!bucketExists)
            {
                await _minioClient.MakeBucketAsync(
                    new MakeBucketArgs()
                    .WithBucket(_settings.BucketName));
            }

            using var stream = file.OpenReadStream();

            await _minioClient.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectKey)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(file.ContentType));

            return objectKey;
        }
    }
}
