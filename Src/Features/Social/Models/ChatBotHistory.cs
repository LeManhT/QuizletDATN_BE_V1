namespace Quizlet_App_Server.Src.Features.Social.Models
{
    // Models/ChatHistory.cs
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;
    using System;
    using System.Text.Json.Serialization;

    public class ChatBotHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("sessionId")]
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("messages")]
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        [BsonElement("createdAt")]
        //[JsonIgnore]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        //[JsonIgnore]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChatMessage
    {
        [BsonElement("message")]
        public string Message { get; set; }

        [BsonElement("response")]
        public string Response { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
