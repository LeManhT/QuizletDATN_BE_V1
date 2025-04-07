using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ChatId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("participants")]
        public List<string> Participants { get; set; } = new List<string>(); // ID của những người trong cuộc trò chuyện.

        [BsonElement("messages")]
        public List<Message> Messages { get; set; } = new List<Message>(); // Danh sách tin nhắn.

        [BsonElement("last_updated")]
        public long LastUpdated { get; set; } = TimeHelper.UnixTimeNow; // Thời điểm cập nhật gần nhất.
    }
}
