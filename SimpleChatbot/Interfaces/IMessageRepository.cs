using SimpleChatbot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.Interfaces
{
    public record MemoryRow(Guid Id, string Content, string Role, DateTime CreatedAt, double Distance);
    public interface IMessageRepository
    {
        Task AddWithEmbeddingAsync(Message message, float[] embedding, CancellationToken ct);
        Task<List<MemoryRow>> SearchByEmbeddingAsync(string userId, float[] queryEmbedding, int topK, CancellationToken ct);
        Task<List<Message>> GetMessagesByEmbeddingAsync(string userId, float[] queryEmbedding, int topK, CancellationToken ct);
        Task<List<Message>> GetRecentMessagesAsync(Guid conversationId, int take, CancellationToken ct);
    }
}
