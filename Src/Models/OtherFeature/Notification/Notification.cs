using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Src.Models.OtherFeature.Notification
{
    [BsonIgnoreExtraElements]
    public class Notification
    {
        [BsonElement("Id")] public int? Id { get; set; } = 0;
        [BsonElement("Was_Pushed")] public bool WasPushed { get; set; } = false;
        [BsonElement("Title")] public string Title { get; set; } = string.Empty;
        [BsonElement("Detail")] public string Detail { get; set; } = string.Empty;
    }

}
