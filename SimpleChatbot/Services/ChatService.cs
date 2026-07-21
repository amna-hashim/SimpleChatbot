using Microsoft.SemanticKernel.ChatCompletion;
using SimpleChatbot.DTOs;
using SimpleChatbot.Interfaces;
using SimpleChatbot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleChatbot.Services
{
    public interface IChatService
    {
        Task<ConversationDto> AddConversationAsync(string userId, CreateConversationDto request, CancellationToken ct);
        Task<MessageDto> AddMessageAsync(string userId, CreateMessageDto request, CancellationToken ct);
        Task<MessageDto> AddMessageWithPastContextAsync(string userId, CreateMessageDto request, CancellationToken ct);
        Task<List<MemoryResultDto>> SearchMemoryAsync(string userId, string queryText, int topK, CancellationToken ct);
    }

    public class ChatService : IChatService
    {
        private readonly IMessageRepository _messageRepo;
        private readonly IConversationRepository _conversationRepo;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChatCompletionService _chatCompletionService;

        public ChatService(
            IMessageRepository messageRepo,
            IConversationRepository conversationRepo,
            IEmbeddingService embeddingService,
            IChatCompletionService chatCompletionService)
        {
            _messageRepo = messageRepo;
            _conversationRepo = conversationRepo;
            _embeddingService = embeddingService;
            _chatCompletionService = chatCompletionService;
        }

        public async Task<ConversationDto> AddConversationAsync(string userId, CreateConversationDto request, CancellationToken ct)
        {
            var conversation = new Conversation()
            {
                ConversationId = Guid.NewGuid(),
                UserId = userId,
                Title = request.Title,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow

            };
            await _conversationRepo.AddAsync(conversation, ct);

            return new ConversationDto(conversation.ConversationId, request.Title, userId, DateTime.UtcNow, new List<MessageDto>());
        }

        public async Task<MessageDto> AddMessageAsync(string userId, CreateMessageDto request, CancellationToken ct)
        {
            var conversation = await _conversationRepo.GetByIdAsync(request.ConversationId, ct)
                ?? throw new InvalidOperationException("Conversation not found.");

            if (conversation.UserId != userId)
                throw new UnauthorizedAccessException();

            //1. Save user message
            var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Content, ct);

            var message = new Message
            {
                MessageId = Guid.NewGuid(),
                ConversationId = request.ConversationId,
                UserId = userId,
                Role = request.Role,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepo.AddWithEmbeddingAsync(message, embedding, ct);


            return new MessageDto(message.MessageId, message.ConversationId, message.Role, message.Content, message.CreatedAt);
        }

        public async Task<MessageDto> AddMessageWithPastContextAsync(string userId, CreateMessageDto request, CancellationToken ct)
        {
            var conversation = await _conversationRepo.GetByIdAsync(request.ConversationId, ct)
                ?? throw new InvalidOperationException("Conversation not found.");

            if (conversation.UserId != userId)
                throw new UnauthorizedAccessException();

            //1. Save user message
            var embedding = await _embeddingService.GenerateEmbeddingAsync(request.Content, ct);

            var message = new Message
            {
                MessageId = Guid.NewGuid(),
                ConversationId = request.ConversationId,
                UserId = userId,
                Role = request.Role,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _messageRepo.AddWithEmbeddingAsync(message, embedding, ct);

            //2.  Get recent conversation history for context
            var recentHistory = await _messageRepo.GetMessagesByEmbeddingAsync(userId, embedding, topK: 20, ct);

            // 3. Generate the assistant's reply
            var replyText = await _chatCompletionService.GetReplyAsync(recentHistory, request.Content, ct);

            // 4. Save the assistant's message too — embedded, same conversation
            var assistantEmbedding = await _embeddingService.GenerateEmbeddingAsync(replyText, ct);
            var assistantMessage = new Message
            {
                MessageId = Guid.NewGuid(),
                ConversationId = request.ConversationId,
                UserId = userId,             // still scoped to the same user for memory search
                Role = "assistant",
                Content = replyText,
                CreatedAt = DateTime.UtcNow
            };
            await _messageRepo.AddWithEmbeddingAsync(assistantMessage, assistantEmbedding, ct);

            return new MessageDto(assistantMessage.MessageId, assistantMessage.ConversationId, assistantMessage.Role, assistantMessage.Content, assistantMessage.CreatedAt);
        }

        public async Task<List<MemoryResultDto>> SearchMemoryAsync(string userId, string queryText, int topK, CancellationToken ct)
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(queryText, ct);
            var rows = await _messageRepo.SearchByEmbeddingAsync(userId, queryEmbedding, topK, ct);

            return rows.Select(r => new MemoryResultDto(r.Id, r.Content, r.Role, r.CreatedAt, r.Distance)).ToList();
        }
    }
}
