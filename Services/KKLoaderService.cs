using AniCard.Models.DTOs;
using AniCard.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace AniCard.Services
{
    public class KKLoaderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KKLoaderService> _logger;

        public KKLoaderService(HttpClient httpClient, ILogger<KKLoaderService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CharacterMetadataResult> GetCharacterMetadataAsync(IFormFile file)
        {
            _logger.LogInformation("KKLoader metadata request started for file {FileName}", file.FileName);
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/metadata", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("KKLoader metadata request failed with status {StatusCode} for file {FileName}", response.StatusCode, file.FileName);
                throw new InvalidCharacterException(errorBody);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<CharacterMetadataResult>(json)
                         ?? throw new InvalidCharacterException("Invalid response from metadata service.");

            _logger.LogInformation("KKLoader metadata request succeeded for file {FileName}", file.FileName);
            return result;
        }
    }
}
