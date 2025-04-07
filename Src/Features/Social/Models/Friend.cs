using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
        [BsonIgnoreExtraElements]
        public class Friend
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

            [BsonElement("user_ids")]
             public List<string> UserIds { get; set; } = new List<string>();

            [BsonElement("created_at")]
            public long CreatedAt { get; set; } = TimeHelper.UnixTimeNow; // Thời gian kết bạn
        }
}
