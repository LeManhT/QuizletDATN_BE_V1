using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Quizlet_App_Server.Src.Features.Social.Models
{
    public record GroupMember
    {
        public string UserId { get; init; } = string.Empty;

        public string UserName { get; init; } = string.Empty;

        public string? AvatarUrl { get; init; }

        public string Role { get; init; } = "MEMBER"; // ADMIN, MEMBER

        public long JoinedAt { get; init; }
    }
}
