using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
    [BsonIgnoreExtraElements]
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("post_id")]
        public string PostId { get; set; } = string.Empty; // ID bài viết mà comment thuộc về

        [BsonElement("author_id")]
        public string AuthorId { get; set; } = string.Empty; // ID của tác giả comment

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty; // Nội dung của comment

        [BsonElement("created_at")]
        public long CreatedAt { get; set; } = TimeHelper.UnixTimeNow; // Thời gian tạo (timestamp)

        [BsonElement("parent_comment_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentCommentId { get; set; }

        [BsonElement("replies_count")]
        public int RepliesCount { get; set; } = 0; // Số lượng phản hồi (dùng để tối ưu truy vấn)

        [BsonElement("is_deleted")]
        public bool IsDeleted { get; set; } = false; // Đánh dấu nếu comment bị xóa
    }
}
