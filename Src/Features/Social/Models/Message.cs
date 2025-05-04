using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string MessageId { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("sender_id")]
        public string SenderId { get; set; } = string.Empty;
        [BsonElement("group_id")]
        public string GroupId { get; set; }

        [BsonElement("receiver_id")]
        public string ReceiverId { get; set; } = string.Empty; 
        [BsonElement("content")]
        public string Content { get; set; } = string.Empty; 

        [BsonElement("timestamp")]
        public long Timestamp { get; set; } = TimeHelper.UnixTimeNow;

        [BsonElement("conversation_id")]
        public string ConversationId { get; set; } = string.Empty;

        [BsonElement("isRead")]
        public bool IsRead { get; set; } = false;

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; } = false;

 
        [BsonElement("attachments")]
        public List<Attachment>? Attachments { get; set; } = new List<Attachment>();

        [BsonElement("isPinned")]
        public bool IsPinned { get; set; } = false;

    }

    public class Attachment
    {
        [BsonElement("type")]
        public string Type { get; set; } = string.Empty; 

        [BsonElement("url")]
        public string Url { get; set; } = string.Empty; 

        [BsonElement("file_name")]
        public string FileName { get; set; } = string.Empty; 

        [BsonElement("file_size")]
        public long FileSize { get; set; } = 0; // Kích thước file
    }

    // Các request models
    public class CreateGroupRequest
    {
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public string? Description { get; set; }
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();
        public string? GroupAvatar { get; set; }
        public bool IsPublic { get; set; } = false;
        public int? MaxMembers { get; set; }
    }

    public class AddMembersRequest
    {
        public string RequestedById { get; set; }
        public List<string> MemberIds { get; set; }
    }

    public class ManageAdminRequest
    {
        public string RequestedById { get; set; }
        public string UserId { get; set; }
        public bool IsAddAdmin { get; set; }
    }

    public class UpdateGroupInfoRequest
    {
        public string RequestedById { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? GroupAvatar { get; set; }
        public bool? IsPublic { get; set; }
        public int? MaxMembers { get; set; }
    }


}
