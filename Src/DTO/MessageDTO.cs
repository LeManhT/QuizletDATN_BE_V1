using Quizlet_App_Server.Src.Features.Social.Models;

namespace Quizlet_App_Server.Src.DTO
{
    public class MessageDTO
    {
        public string? MessageId { get; set; } // Cho phép null trong trường hợp DTO không cần ID

        public string SenderId { get; set; } = string.Empty;

        public string ReceiverId { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public long Timestamp { get; set; }

        public string ConversationId { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public bool IsDeleted { get; set; }

        public List<AttachmentDTO>? Attachments { get; set; }

        public bool IsPinned { get; set; }
    }

    public class AttachmentDTO
    {
        public string Type { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public long FileSize { get; set; }
    }
}
