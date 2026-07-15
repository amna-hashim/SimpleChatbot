using SimpleChatbot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.Services
{
    public interface IChatCompletionService
    {
        Task<string> GetReplyAsync(List<Message> history, string newUserMessage, CancellationToken ct);
    }

    public class OpenAiChatCompletionService : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private const string Model = "gpt-4o";

        public OpenAiChatCompletionService(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<string> GetReplyAsync(List<Message> history, string newUserMessage, CancellationToken ct)
        {
            var messages = history
                .OrderBy(m => m.CreatedAt)
                .Select(m => new { role = m.Role.ToLower(), content = m.Content })
                .Append(new { role = "user", content = newUserMessage })
                .ToList();

            var response = await _httpClient.PostAsJsonAsync("chat/completions", new
            {
                model = Model,
                messages
            }, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                throw new Exception($"Chat completion failed ({response.StatusCode}): {errorBody}");
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: ct);
            return result!.Choices[0].Message.Content;
        }

        private record ChatCompletionResponse(List<Choice> Choices);
        private record Choice(ChatMessage Message);
        private record ChatMessage(string Content);
    }

}
