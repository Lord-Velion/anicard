using AniCard.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace AniCard.Repositories
{
    /// <summary>
    /// Repository for file storage operations in MinIO/S3-compatible object storage.
    /// Handles upload, download, and deletion of character card PNG files.
    /// </summary>
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

        /// <summary>
        /// Uploads the character file to the configured MinIO/S3 bucket.
        /// Creates the bucket if it does not already exist.
        /// </summary>
        /// <param name="file">The uploaded file stream.</param>
        /// <returns>The generated object key
        /// (format: "{guid}-{original-filename}").</returns>
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

        /// <summary>
        /// Downloads a character card file from MinIO object storage by object key.
        /// Loads the entire file into a memory stream.
        /// </summary>
        /// <param name="objectKey">The MinIO object key to download</param>
        /// <returns>
        /// A tuple containing the file memory stream and the card name
        /// (derived from the object key).
        /// </returns>
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

        /// <summary>
        /// Deletes the character's raw file from MinIO object storage.
        /// </summary>
        /// <param name="objectKey">The MinIO object key of the file to
        /// delete.</param>
        public async Task DeleteCharacterAsync(string objectKey)
        {
            _logger.LogInformation("Deleting object {ObjectKey} from bucket {BucketName}",
                objectKey, _settings.BucketName);

            await _minioClient.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectKey));

            _logger.LogInformation("MinIO deletion completed for object key {ObjectKey}", objectKey);
        }
    }
}
