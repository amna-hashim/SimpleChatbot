using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SimpleChatbot.Models
{
    public class Conversation
    {
        public Guid ConversationId { get; set; }
        public string UserId { get; set; } = default!;
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Message> Messages { get; set; } = new();
    }
}
