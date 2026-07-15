using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using SimpleChatbot.Interfaces;
using SimpleChatbot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;

namespace SimpleChatbot.Infrastructure
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ChatDBContext _db;   // used for the connection string
        public MessageRepository(ChatDBContext db) => _db = db;

        public async Task AddWithEmbeddingAsync(Message message, float[] embedding, CancellationToken ct)
        {
            var vectorJson = JsonSerializer.Serialize(embedding);
            var conn = (SqlConnection)_db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(@"
            INSERT INTO Messages (MessageId, ConversationId, UserId, Role, Content, Embedding, TokenCount, CreatedAt)
            VALUES (@MessageId, @ConversationId, @UserId, @Role, @Content, CAST(@Embedding AS VECTOR(1536)), @TokenCount, @CreatedAt)",
                conn);

            cmd.Parameters.AddWithValue("@MessageId", message.MessageId);
            cmd.Parameters.AddWithValue("@ConversationId", message.ConversationId);
            cmd.Parameters.AddWithValue("@UserId", message.UserId);
            cmd.Parameters.AddWithValue("@Role", message.Role);
            cmd.Parameters.AddWithValue("@Content", message.Content);
            cmd.Parameters.AddWithValue("@Embedding", vectorJson);
            cmd.Parameters.AddWithValue("@TokenCount", (object?)message.TokenCount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", message.CreatedAt);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<List<MemoryRow>> SearchByEmbeddingAsync(
            string userId, float[] queryEmbedding, int topK, CancellationToken ct)
        {
            var vectorJson = JsonSerializer.Serialize(queryEmbedding);
            var conn = (SqlConnection)_db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(@"
            SELECT TOP (@TopK)
                MessageId, Content, Role, CreatedAt,
                VECTOR_DISTANCE('cosine', Embedding, CAST(@QueryVector AS VECTOR(1536))) AS Distance
            FROM Messages
            WHERE UserId = @UserId AND Embedding IS NOT NULL
            ORDER BY Distance ASC", conn);

            cmd.Parameters.AddWithValue("@TopK", topK);
            cmd.Parameters.AddWithValue("@QueryVector", vectorJson);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var results = new List<MemoryRow>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                results.Add(new MemoryRow(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetDateTime(3),
                    reader.GetDouble(4)));
            }
            return results;
        }

        public async Task<List<Message>> GetMessagesByEmbeddingAsync(
          string userId, float[] queryEmbedding, int topK, CancellationToken ct)
        {
            var vectorJson = JsonSerializer.Serialize(queryEmbedding);
            var conn = (SqlConnection)_db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(@"
            SELECT TOP (@TopK) *,
            VECTOR_DISTANCE('cosine', Embedding, CAST(@QueryVector AS VECTOR(1536))) AS Distance
            FROM Messages
            WHERE UserId = @UserId AND Embedding IS NOT NULL
            AND VECTOR_DISTANCE('cosine', Embedding, CAST(@QueryVector AS VECTOR(1536))) < @MaxDistance
            ORDER BY Distance ASC", conn);

            cmd.Parameters.AddWithValue("@TopK", topK);
            cmd.Parameters.AddWithValue("@QueryVector", vectorJson);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@MaxDistance", SimpleChatbot.Shared.Constants.MaxDistance);

            var results = new List<Message>();
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                Message message = new Message();
                message.MessageId = reader.GetGuid(reader.GetOrdinal("MessageId"));
                message.ConversationId = reader.GetGuid(reader.GetOrdinal("ConversationId"));
                message.UserId = reader.GetString(reader.GetOrdinal("UserId"));
                message.Role = reader.GetString(reader.GetOrdinal("Role"));
                message.Content = reader.GetString(reader.GetOrdinal("Content"));
                //message.Embedding = JsonSerializer.Deserialize<float[]>(reader.GetOrdinal("Embedding"));

             
                results.Add(message);
            }
            return results;
        }

        public async Task<List<Message>> GetRecentMessagesAsync(Guid conversationId, int take, CancellationToken ct)
        {
            return await _db.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(take)
                .OrderBy(m => m.CreatedAt) // re-order chronologically after taking the most recent N
                .ToListAsync(ct);
        }

    }
}
