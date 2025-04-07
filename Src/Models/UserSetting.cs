using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class UserSetting
    {
        [BsonElement("dark_mode")] public bool DarkMode { get; set; } = false;
        [BsonElement("notification")] public bool Notification { get; set; } = false;
    }
}
