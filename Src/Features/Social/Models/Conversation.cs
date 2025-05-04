using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
    public record Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("type")]
        public string Type { get; set; } = "personal";

        [BsonElement("members")]
        public List<GroupMember> Members { get; set; } = new List<GroupMember>();

        [BsonElement("admins")]
        public List<string> Admins { get; set; } = new List<string>();

        [BsonElement("last_message")]
        public string? LastMessage { get; set; }

        public int ParticipantCount { get; init; } = 0;

        [BsonElement("last_message_time")]
        public long LastMessageTime { get; set; } = TimeHelper.UnixTimeNow;

        [BsonElement("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [BsonElement("is_active")]
        public bool IsActive { get; set; } = true;

        [BsonElement("created_at")]
        public long CreatedAt { get; set; } = TimeHelper.UnixTimeNow;

        [BsonElement("updated_at")]
        public long UpdatedAt { get; set; } = TimeHelper.UnixTimeNow;

        // Thêm các thuộc tính mới
        [BsonElement("creator_id")]
        public string? CreatorId { get; set; }

        [BsonElement("group_avatar")]
        public string? GroupAvatar { get; set; }

        [BsonElement("is_public")]
        public bool IsPublic { get; set; } = false;

        [BsonElement("max_members")]
        public int MaxMembers { get; set; } = 100; // Giới hạn mặc định

        [BsonElement("join_code")]
        public string? JoinCode { get; set; } // Mã để tham gia nhóm

        [BsonElement("description")]
        public string? Description { get; set; }

        // Danh sách các yêu cầu tham gia chờ phê duyệt
        [BsonElement("join_requests")]
        public List<string> JoinRequests { get; set; } = new List<string>();

        // Đăng ký thành viên bị cấm
        [BsonElement("banned_members")]
        public List<string> BannedMembers { get; set; } = new List<string>();

        // Cài đặt quyền riêng tư và thông báo
        [BsonElement("privacy_settings")]
        public ConversationPrivacySettings PrivacySettings { get; set; } = new ConversationPrivacySettings();
    }

    // Lớp con để quản lý cài đặt riêng tư
    public class ConversationPrivacySettings
    {
        [BsonElement("who_can_add_members")]
        public string WhoCanAddMembers { get; set; } = "admins"; // "admins", "all_members"

        [BsonElement("who_can_send_messages")]
        public string WhoCanSendMessages { get; set; } = "all_members"; // "all_members", "admins_only"

        [BsonElement("is_searchable")]
        public bool IsSearchable { get; set; } = true;

        [BsonElement("allow_link_invites")]
        public bool AllowLinkInvites { get; set; } = true;
    }
}