using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.ChatBot.Models
{
    public class ChatBotMessage
    {
        public int Id { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public long Timestamp { get; set; } = TimeHelper.UnixTimeNow;
    }
}
