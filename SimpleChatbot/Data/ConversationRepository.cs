using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleChatbot.Interfaces;
using SimpleChatbot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;

namespace SimpleChatbot.Infrastructure
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ChatDBContext _db;
        public ConversationRepository(ChatDBContext db) => _db = db;

        public async Task AddAsync(Conversation conversation, CancellationToken ct)
        {
            var conn = (SqlConnection)_db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand(@"
                                               INSERT INTO [dbo].[Conversations]
                                               ([ConversationId]
                                               ,[UserId]
                                               ,[Title]
                                               ,[CreatedAt]
                                               ,[UpdatedAt])
                                         VALUES
                                               (@ConversationId
                                               ,@UserId
                                               ,@Title
                                               ,@CreatedAt
                                               ,@UpdatedAt);",
                                                  conn);

            cmd.Parameters.AddWithValue("@ConversationId", conversation.ConversationId);
            cmd.Parameters.AddWithValue("@UserId", conversation.UserId);
            cmd.Parameters.AddWithValue("@Title", conversation.Title);
            cmd.Parameters.AddWithValue("@CreatedAt", conversation.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", conversation.UpdatedAt);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct) =>
            _db.Conversations.FirstOrDefaultAsync(c => c.ConversationId == id, ct);

        public Task<Conversation?> GetWithMessagesAsync(Guid id, CancellationToken ct) =>
            _db.Conversations.Include(c => c.Messages).FirstOrDefaultAsync(c => c.ConversationId == id, ct);


    }
}
