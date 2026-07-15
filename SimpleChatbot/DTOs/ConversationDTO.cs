using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.DTOs
{
    //Outbound
    public record ConversationDto(
        Guid Id, string Title, string UserId,
        DateTime CreatedAt, List<MessageDto> Messages);
    //Inbound
    public record CreateConversationDto(string Title, string UserId);
}
