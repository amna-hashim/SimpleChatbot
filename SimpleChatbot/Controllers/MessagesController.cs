using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleChatbot.DTOs;
using SimpleChatbot.Models;
using SimpleChatbot.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;


namespace SimpleChatbot.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : BaseController
    {
        private readonly IChatService _chatService;

        public MessagesController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ConversationDto>> CreateNewMessage(string conversationId, [FromBody] CreateMessageDto request, CancellationToken ct)
        {
            if (conversationId.ToLower() != request.ConversationId.ToString().ToLower())
                return BadRequest("Route and body conversationId mismatch.");

            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = "testUser123";
            var result = await _chatService.AddMessageWithPastContextAsync(userId, request, ct);
            return Ok(result);
        }

        [HttpGet("memory/search")]
        public async Task<ActionResult<List<MemoryResultDto>>> SearchMemory([FromQuery] string query, [FromQuery] int topK = 8, CancellationToken ct = default)
        {
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var userId = "testUser123";
            var results = await _chatService.SearchMemoryAsync(userId, query, topK, ct);
            return Ok(results);
        }
    }
}
