using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Models
{
    [BsonIgnoreExtraElements]
    public class Configurable
    {
        [BsonElement("version")] public int Version { get; set; } = 0;
        [BsonElement("special_name")] public virtual string SpecialName { get; set; } = string.Empty;
    }
}
