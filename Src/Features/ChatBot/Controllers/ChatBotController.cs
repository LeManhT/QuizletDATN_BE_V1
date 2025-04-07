using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Src.Controllers;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.Features.ChatBot.Models;
using Quizlet_App_Server.Src.Features.ChatBot.Services;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.ChatBot.Controllers
{
    public class ChatBotController : ControllerExtend<ChatBotMessage>
    {
        private readonly ChatBotService _chatBotService;
        private readonly ILogger<ChatBotController> _logger;
        public ChatBotController(AppConfigResource userStoreDatabaseSetting, IMongoClient mongoClient, IConfiguration configuration, ILogger<ChatBotController> logger) : base(userStoreDatabaseSetting, mongoClient)
        {
            _chatBotService = new ChatBotService(mongoClient, configuration, logger);
            _logger = logger;
        }

        //// API gửi tin nhắn và nhận phản hồi từ AI
        //[HttpPost("send")]
        //public async Task<IActionResult> SendMessage([FromBody] ChatBotMessage userMessage)
        //{
        //    if (userMessage == null || string.IsNullOrWhiteSpace(userMessage.Message))
        //        return BadRequest("Message is required.");

        //    // Gửi tin nhắn đến AI
        //    var aiResponse = await _chatBotService.GetAIResponseAsync(userMessage.Message);

        //    _logger?.LogInformation("User message: {Message}", userMessage);


        //    if (aiResponse == null)
        //        return StatusCode(500, "AI service failed.");

        //    // Lưu tin nhắn của người dùng
        //    userMessage.Sender = "User";
        //    userMessage.Timestamp = TimeHelper.UnixTimeNow;
        //    await _chatBotService.saveChatBotMessageAsync(userMessage);

        //    // Lưu tin nhắn phản hồi của AI
        //    var aiMessage = new ChatBotMessage
        //    {
        //        Sender = "AI",
        //        Message = aiResponse,
        //        Timestamp = TimeHelper.UnixTimeNow
        //    };
        //    await _chatBotService.saveChatBotMessageAsync(aiMessage);

        //    return Ok(new
        //    {
        //        userMessage = userMessage.Message,
        //        aiResponse
        //    });
        //}

        // API lấy lịch sử chat
        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory()
        {
            var history = await _chatBotService.getListChatHistoryAsync();
            return Ok(history);
        }
    }
}
