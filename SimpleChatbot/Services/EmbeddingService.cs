using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace SimpleChatbot.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct);
    }

    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private const string Model = "text-embedding-3-small"; // 1536 dims

        public OpenAIEmbeddingService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)
        {
            var response = await _httpClient.PostAsJsonAsync("embeddings", new
            {
                model = Model,
                input = text
            }, ct);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>(cancellationToken: ct);
            return result!.Data[0].Embedding;
        }

        private record OpenAIEmbeddingResponse(List<OpenAIEmbeddingData> Data);
        private record OpenAIEmbeddingData(float[] Embedding);
    }

    public class GitHubModelsEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private const string Model = "text-embedding-3-small"; // 1536 dims

        public GitHubModelsEmbeddingService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)
        {
            var response = await _httpClient.PostAsJsonAsync("embeddings", new
            {
                model = Model,
                input = text
            }, ct);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken: ct);
            return result!.Data[0].Embedding;
        }

        private record EmbeddingResponse(List<EmbeddingData> Data);
        private record EmbeddingData(float[] Embedding);
    }
}
