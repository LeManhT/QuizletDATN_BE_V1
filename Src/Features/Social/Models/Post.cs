using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Src.Models;
using Quizlet_App_Server.Utility;
using Quizlet_App_Server.Src.DTO;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
    [BsonIgnoreExtraElements]
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("authorId")] 
        public string AuthorId { get; set; } = string.Empty;

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("created_at")] 
        public long CreatedAt { get; set; } = TimeHelper.UnixTimeNow;

        [BsonElement("comments")] 
        public List<Comment> Comments { get; set; } = new List<Comment>();

        [BsonElement("likes")]
        //public List<string> Likes { get; set; } = new List<string>();
        public int Likes { get; set; }

        [BsonElement("fileUrls")] public List<string> FileUrls { get; set; } = new List<string>();

        [BsonElement("imageUrls")] public List<string> ImageUrls { get; set; } = new List<string>();
        [BsonElement("author")] public string Author { get; set; } = string.Empty;
        [BsonElement("likeByUsers")] public List<string> LikedByUsers { get; set; } = new List<string>();

        public Post() { }

        public Post(PostDTO dto)
        {
            this.AuthorId = dto.AuthorId;
            this.Content = dto.Content;
            this.CreatedAt = dto.CreatedAt;
            this.Comments = dto.Comments ?? new List<Comment>();
            this.Likes = dto.Likes;
            this.LikedByUsers = dto.LikedByUsers ?? new List<string>();
        }

        public Post Clone(string newId = null)
        {
            Post postClone = this.MemberwiseClone() as Post;

            if (newId != null)
            {
                postClone.Id = newId;
            }

            return postClone;
        }

        public void UpdateInfo(PostDTO postDTO)
        {
            this.Id = this.Id; 
            this.CreatedAt = this.CreatedAt;
            this.AuthorId = postDTO.AuthorId;
            this.Content = postDTO.Content;
            this.Comments = postDTO.Comments ?? new List<Comment>();
            this.Likes = postDTO.Likes;
        }
    }

}