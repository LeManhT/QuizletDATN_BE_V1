using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Models.Helper
{
    public class UserSequence
    {
        [BsonId] public string Id { get; set; }
        public int Value { get; set; }
    }
}
