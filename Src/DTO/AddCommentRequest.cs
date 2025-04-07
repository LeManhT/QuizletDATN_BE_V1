namespace Quizlet_App_Server.Src.DTO
{
    public class AddCommentRequest
    {
        public string PostId { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ParentCommentId { get; set; }
    }
}
