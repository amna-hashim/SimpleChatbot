using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.DTOs
{
    // Memory search result
    public record MemoryResultDto(Guid MessageId, string Content, string Role, DateTime CreatedAt, double Distance);
}
