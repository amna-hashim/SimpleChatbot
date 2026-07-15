using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SimpleChatbot.Models
{
    public class Message
    {
        public Guid MessageId { get; set; }
        public Guid ConversationId { get; set; }
        public string UserId { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Content { get; set; } = default!;
        public int? TokenCount { get; set; }
        public DateTime CreatedAt { get; set; }

        // NOT mapped by EF — populated manually via raw SQL when needed
        [NotMapped]
        public float[]? Embedding { get; set; }
        public Conversation Conversation { get; set; } = default!;
    }
}
