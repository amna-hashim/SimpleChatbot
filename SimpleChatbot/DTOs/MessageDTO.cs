using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.DTOs
{
    // Outbound: what the client sees back
    public record MessageDto(Guid Id, Guid ConversationId, string Role, string Content, DateTime CreatedAt);
    // Inbound: what the client sends to post a message
    public record CreateMessageDto(Guid ConversationId, string Role, string Content);
}
