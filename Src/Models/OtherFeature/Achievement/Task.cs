using MongoDB.Bson.Serialization.Attributes;

namespace Quizlet_App_Server.Src.Models
{
    [BsonIgnoreExtraElements]
    public class Task
    {
        [BsonElement("task_id")] public int? Id { get; set; } = 0;
        [BsonElement("task_name")] public string? TaskName { get; set; } = string.Empty;
        [BsonElement("type")] public string? Type { get; set; } = TaskType.None;
        [BsonElement("score")] public int? Score { get; set; } = 1;
        [BsonElement("status")]
        public TaskStatus Status
        {
            get
            {
                if (Progress >= Condition) return TaskStatus.Completed;
                else if (Progress <= 0) return TaskStatus.NONE;
                else return TaskStatus.InProgress;
            }
        }
        [BsonElement("description")] public string? Description { get; set; } = string.Empty;
        [BsonElement("condition")] public int? Condition { get; set; } = 0;
        [BsonElement("progress")] public int Progress { get; set; } = 0;
    }
    public enum TaskStatus
    {
        NONE = 0,
        InProgress = 1,
        Completed = 2
    }
    public static class TaskType
    {
        public const string None = "None";
        public const string STREAK = "Streak";
        public const string STUDY = "Study";
    }
}
