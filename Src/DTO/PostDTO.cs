using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.DTO
{
    [Serializable]
    public class PostDTO
    {
        [BsonElement("author_id")] public string AuthorId { get; set; } = string.Empty;
        [BsonElement("author")] public string Author { get; set; } = string.Empty;
        [BsonElement("content")] public string Content { get; set; } = string.Empty;
        [BsonElement("created_at")] public long CreatedAt { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("comments")] public List<Comment> Comments { get; set; } = new List<Comment>();
        //[BsonElement("likes")] public List<string> Likes { get; set; } = new List<string>();
        public int Likes { get; set; } = 0;
        [BsonElement("imageUrls")] public List<string> ImageUrls { get; set; } = new List<string>();
        [BsonElement("fileUrls")] public List<string> FileUrls { get; set; } = new List<string>();
        [BsonElement("likeByUsers")] public List<string> LikedByUsers { get; set; } = new List<string>();
    }
}
