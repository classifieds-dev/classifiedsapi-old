using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Chat.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Chat.Controllers
{
    [ApiController]
    [Authorize(Policy = "RegisteredUser")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ChatRepository _chatRepository;

        public ChatController(ILogger<ChatController> logger, ChatRepository chatRepository)
        {
            _logger = logger;
            _chatRepository = chatRepository;
        }

        [HttpGet("chatconversations")]
        public async Task<List<ChatConversation>> Conversations()
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;
            return await _chatRepository.GetConversations(token.Subject);
        }

        [HttpGet("chatmessages")]
        public async Task<List<ChatMessage>> Messages(string recipientId)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;
            return await _chatRepository.GetMessages(token.Subject, recipientId);
        }

        [HttpPost("chatmessage")]
        public async Task<ActionResult<ChatMessage>> Message(ChatMessage message)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;
            message.SenderId = token.Subject;
            await _chatRepository.CreateMessage(message);
            return message;
        }

        [Route("connect")]
        public async Task<ActionResult> Connect()
        {
            var request = HttpContext.Items["LambdaRequestObject"] as APIGatewayProxyRequest;
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;
            // var context = HttpContext.Items["LambdaContext"] as ILambdaContext;
            await _chatRepository.CreateConnection(request.RequestContext.ConnectionId, token.Subject);
            return StatusCode(200);
        }

        [Route("disconnect")]
        public async Task<ActionResult> disconnect()
        {
            var request = HttpContext.Items["LambdaRequestObject"] as APIGatewayProxyRequest;
            await _chatRepository.DeleteConnection(request.RequestContext.ConnectionId);
            return StatusCode(200);
        }

    }
}
