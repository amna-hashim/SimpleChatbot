using SimpleChatbot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.Interfaces
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Conversation?> GetWithMessagesAsync(Guid id, CancellationToken ct);

        Task AddAsync(Conversation conversation, CancellationToken ct);
    }
}
