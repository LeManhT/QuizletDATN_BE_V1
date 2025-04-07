using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.DTO
{
    public class CommentDTO
    {
        public string Id { get; set; } = string.Empty; 
        public string PostId { get; set; } = string.Empty; 
        public string AuthorId { get; set; } = string.Empty; 
        public string Content { get; set; } = string.Empty;
        public long CreatedAt { get; set; } = TimeHelper.UnixTimeNow; 
        public string? ParentCommentId { get; set; } 
        public int RepliesCount { get; set; } = 0; 
        public bool IsDeleted { get; set; } = false; 
    }
}
