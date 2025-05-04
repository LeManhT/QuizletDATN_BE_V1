using AutoMapper.Configuration.Annotations;
using MongoDB.Bson;
using Quizlet_App_Server.Src.Features.Social.Models;

namespace Quizlet_App_Server.Src.DTO
{
    public record GroupMessageDTO
    {
        public string? MessageId { get; init; }
        public string SenderId { get; init; } = string.Empty;
        public string GroupId { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public bool IsRead { get; init; } = false;
        public bool IsDeleted { get; init; } = false;
        public List<AttachmentDTO> Attachments { get; init; } = new List<AttachmentDTO>();
        public bool IsPinned { get; init; } = false;
    }

    public record ConversationDTO
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }

        public string Type { get; set; } = "personal";

        public List<GroupMember>? Members { get; set; }

        public List<string>? Admins { get; set; }

        public string? LastMessage { get; set; }

        public int ParticipantCount { get; set; }

        public long LastMessageTime { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        public long CreatedAt { get; set; }

        public long UpdatedAt { get; set; }

        public string? CreatorId { get; set; }

        public string? GroupAvatar { get; set; }

        public bool IsPublic { get; set; }

        public int MaxMembers { get; set; }

        public string? JoinCode { get; set; }

        public string? Description { get; set; }

        public List<string>? JoinRequests { get; set; }

        public List<string>? BannedMembers { get; set; }
    }

    public record GroupMemberDTO
    {
        public string UserId { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string? AvatarUrl { get; init; }
        public string Role { get; init; } = "MEMBER";
        public long JoinedAt { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
