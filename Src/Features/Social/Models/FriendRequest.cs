using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
    [BsonIgnoreExtraElements]
    public class FriendRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("sender_id")]
        public string SenderId { get; set; } = string.Empty; // ID người gửi lời mời

        [BsonElement("receiver_id")]
        public string ReceiverId { get; set; } = string.Empty; // ID người nhận lời mời


        [BsonElement("sender_name")]
        public string SenderName { get; set; } = string.Empty; 


        [BsonElement("receiver_name")]
        public string ReceiverName { get; set; } = string.Empty; 

        [BsonElement("created_at")]
        public long CreatedAt { get; set; } = TimeHelper.UnixTimeNow; // Thời gian gửi lời mời


        [BsonElement("mutual_friends")]
        public long MutualFriends { get; set; } = 0; // Thời gian gửi lời mời

        [BsonElement("status")]
        public string Status { get; set; } = "pending"; // "pending", "accepted", "rejected"
    }
}