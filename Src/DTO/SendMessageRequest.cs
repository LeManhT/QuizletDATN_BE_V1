namespace Quizlet_App_Server.Src.DTO
{
        public class SendMessageRequest
        {
            public Guid SenderId { get; set; }
            public Guid ReceiverId { get; set; }
            public string Content { get; set; }
        }

}
