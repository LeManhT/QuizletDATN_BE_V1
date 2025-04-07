using MongoDB.Bson.Serialization.Attributes;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class Streak
    {
        [BsonElement("last_time")] public long LastTime { get; set; } = TimeHelper.UnixTimeNow;
        [BsonElement("current_streak")] public int CurrentStreak { get; set; } = 1;
    }

    [Serializable]
    public class StreakRespone
    {
        public Streak Streak { get; set; } = new Streak();
        public Achievement Achievement { get; set; } = new Achievement();
    }
}
