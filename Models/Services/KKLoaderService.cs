using AniCard.Models.DTOs;
using AniCard.Models.Exceptions;
using System.Net.Http;

namespace AniCard.Models.Services
{
    public class KKLoaderService : IKKLoaderService
    {
        private readonly HttpClient _httpClient;

        public KKLoaderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CharacterMetadataResult> GetCharacterMetadataAsync(IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            content.Add(streamContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("/metadata", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new InvalidCharacterException(errorBody);
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<CharacterMetadataResult>(json)
                         ?? throw new InvalidCharacterException("Invalid response from metadata service.");

            return result;
        }
    }
}
