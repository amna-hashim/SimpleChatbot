using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SimpleChatbot.DTOs;
using SimpleChatbot.Services;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;


namespace SimpleChatbot.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : BaseController
    {
        private readonly IChatService _chatService;

        public ConversationsController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ConversationDto>> CreateNewConversation([FromBody] CreateConversationDto request, CancellationToken ct)
        {
            Logger.LogInformation("CreateNewConversation endpoint");
            var userId = "testUser123";// User.FindFirstValue(ClaimTypes.NameIdentifier)!; // from auth
            var result = await _chatService.AddConversationAsync(userId, request, ct);
            return Ok(result);
        }
    }
}
