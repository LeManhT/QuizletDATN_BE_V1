using Microsoft.AspNetCore.Mvc.ViewEngines;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class Admin
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("user_name")] public string UserName { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;
        [BsonElement("time_created")] public long TimeCreated { get; set; } = TimeHelper.UnixTimeNow;

    }

    [BsonIgnoreExtraElements]
    public class AdminSignUp
    {
        [BsonElement("login_name")] public string LoginName { get; set; } = string.Empty;
        [BsonElement("login_password")] public string LoginPassword { get; set; } = string.Empty;
        [BsonElement("email")] public string Email { get; set; } = string.Empty;

    }
}
