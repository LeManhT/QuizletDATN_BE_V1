namespace Quizlet_App_Server.Src.Features.Social.Controller
{
    // Controllers/ChatController.cs
    using Microsoft.AspNetCore.Mvc;
    using Quizlet_App_Server.Src.Features.ChatBot.Services;
    using Quizlet_App_Server.Src.Features.Social.Models;
    using Quizlet_App_Server.Src.Features.Social.Service;
    using System.Threading.Tasks;

    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly ChatHistoryService _chatHistoryService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(ChatHistoryService chatHistoryService, ILogger<ChatController> logger)
        {
            _chatHistoryService = chatHistoryService;
            this._logger = logger;
        }

        // Lưu tin nhắn vào cuộc hội thoại
        [HttpPost("saveHistory")]
        public async Task<IActionResult> SaveChatMessage([FromBody] ChatMessageRequest request)
        {
            var message = new ChatMessage
            {
                Message = request.Message,
                Response = request.Response,
                Timestamp = DateTime.UtcNow
            };
            _logger.LogInformation("SaveChatMessage");
            await _chatHistoryService.SaveChatMessageAsync(request.UserId, request.SessionId, message);
            return Ok(new { message = "Tin nhắn đã được lưu!" });
        }

        // Lấy lịch sử hội thoại theo userId
        [HttpGet("getHistory/{userId}")]
        public async Task<IActionResult> GetChatHistory(string userId)
        {
            var history = await _chatHistoryService.GetChatHistoryAsync(userId);
            return Ok(history);
        }

        // Xóa lịch sử chat theo userId
        [HttpDelete("deleteHistory/{userId}")]
        public async Task<IActionResult> DeleteChatHistory(string userId)
        {
            var deleted = await _chatHistoryService.DeleteChatHistoryAsync(userId);
            if (deleted)
                return Ok(new { message = "Lịch sử chat đã được xóa!" });

            return NotFound(new { message = "Không tìm thấy lịch sử chat!" });
        }
    }

    public class ChatMessageRequest
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
    }

}
