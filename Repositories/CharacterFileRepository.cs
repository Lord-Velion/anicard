using AniCard.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace AniCard.Repositories
{
    public class CharacterFileRepository
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _settings;
        private readonly ILogger<CharacterFileRepository> _logger;

        public CharacterFileRepository(
            IMinioClient minioClient,
            IOptions<MinioSettings> settings,
            ILogger<CharacterFileRepository> logger)
        {
            _minioClient = minioClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> UploadCharacterAsync(IFormFile file)
        {
            var objectKey = $"{Guid.NewGuid()}-{file.FileName}";
            _logger.LogInformation("MinIO upload started for file {FileName} with object key {ObjectKey}", file.FileName, objectKey);

            var bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs()
                    .WithBucket(_settings.BucketName));

            if (!bucketExists)
            {
                _logger.LogInformation("MinIO bucket {BucketName} not found. Creating bucket.", _settings.BucketName);
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

            _logger.LogInformation("MinIO upload completed for object key {ObjectKey}", objectKey);
            return objectKey;
        }

        public async Task<(Stream FileStream, string CardName)> DownloadCharacterAsync(string objectKey)
        {
            _logger.LogInformation("MinIO download started for object key {ObjectKey}", objectKey);

            var cardName = objectKey;

            var memoryStream = new MemoryStream();

            await _minioClient.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectKey)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream)));

            memoryStream.Position = 0;

            _logger.LogInformation("MinIO download completed for object key {ObjectKey}", objectKey);
            return (memoryStream, cardName);
        }
    }
}
