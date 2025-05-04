using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Quizlet_App_Server.DataSettings;
using Quizlet_App_Server.Src.Controllers;
using Quizlet_App_Server.Src.DataSettings;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Features.Social.Service;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerExtend<Message>
    {
        private readonly MessageService _messageService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly ILogger<MessageController> logger;
        public MessageController(AppConfigResource settings,MessageService message, IMongoClient mongoClient, IConfiguration config, IHubContext<ChatHub> hubContext, ILogger<MessageController> logger) : base(settings, mongoClient)
        {
            _messageService = message;
            _hubContext = hubContext;
            this.logger = logger;
        }

        // API: Lấy danh sách tin nhắn giữa hai người dùng
        [HttpGet("messages")]
        public async Task<IActionResult> GetChatMessages([FromQuery] string userId1, [FromQuery] string userId2)
        {
            if (string.IsNullOrEmpty(userId1) || string.IsNullOrEmpty(userId2))
            {
                return BadRequest("Both user IDs are required.");
            }

            var messages = await _messageService.GetChatMessagesAsync(userId1, userId2);
            return Ok(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesByConversation([FromQuery] string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                return BadRequest("Both user IDs are required.");
            }
            var messages = await _messageService.GetMessagesByConversationAsync(conversationId);
            return Ok(messages);
        }

        // API: Lấy danh sách tin nhắn mới cho một người dùng
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadMessages([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var unreadMessages = await _messageService.GetUnreadMessagesAsync(userId);
            return Ok(unreadMessages);
        }

        //[HttpGet("{conversationId}")]
        //public async Task<IActionResult> GetMessages(string conversationId)
        //{
        //    var messages = await _messageService.GetChatMessagesAsync(conversationId);
        //    return Ok(messages);
        //}

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            await _messageService.DeleteMessageAsync(messageId);
            return NoContent();
        }

        //[HttpPost("create-group")]
        //public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupRequest request)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(request.Name))
        //            return BadRequest("Tên nhóm không được để trống");

        //        if (string.IsNullOrWhiteSpace(request.CreatorId))
        //            return BadRequest("Phải có người tạo nhóm");

        //        var members = request.Members?.Distinct().ToList()
        //            ?? new List<GroupMember> { new GroupMember { UserId = request.CreatorId } };

        //        if (!members.Any(m => m.UserId == request.CreatorId))
        //            members.Add(new GroupMember { UserId = request.CreatorId });

        //        var groupMembers = members.Select(id => new GroupMember
        //        {
        //            UserId = id,
        //            JoinedAt = TimeHelper.UnixTimeNow,
        //            Role = id == request.CreatorId ? "admin" : "member"
        //        }).ToList();

        //        var conversation = new Conversation
        //        {
        //            Name = request.Name,
        //            Type = "group",
        //            CreatorId = request.CreatorId,
        //            Members = groupMembers,
        //            Admins = new List<string> { request.CreatorId },
        //            Description = request.Description,
        //            GroupAvatar = request.GroupAvatar,
        //            IsPublic = request.IsPublic,
        //            MaxMembers = request.MaxMembers ?? 100,
        //            JoinCode = GenerateUniqueJoinCode(),
        //            CreatedAt = TimeHelper.UnixTimeNow,
        //            UpdatedAt = TimeHelper.UnixTimeNow
        //        };

        //        await _messageService.SaveConversationAsync(conversation);

        //        return Ok(new
        //        {
        //            message = "Tạo nhóm thành công",
        //            conversationId = conversation.ConversationId,
        //            joinCode = conversation.JoinCode
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Lỗi tạo nhóm: {ex.Message}");
        //        return StatusCode(500, "Có lỗi xảy ra khi tạo nhóm");
        //    }
        //}


        [HttpGet("GetUserConversations")]
        public async Task<IActionResult> GetUserConversations([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { success = false, message = "User ID is required." });
            }

            try
            {
                var conversations = await _messageService.GetUserConversationsAsync(userId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error retrieving conversations for user {userId}: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("CreateConversation")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserId1) || string.IsNullOrWhiteSpace(request.UserId2))
                {
                    return BadRequest(new { success = false, message = "Both user IDs are required." });
                }

                var existingConversation = await _messageService.GetConversationAsync(request.UserId1, request.UserId2);
                if (existingConversation != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Conversation already exists",
                        data = existingConversation
                    });
                }

                var members = new List<GroupMember>
        {
            new GroupMember { UserId = request.UserId1, JoinedAt = TimeHelper.UnixTimeNow },
            new GroupMember { UserId = request.UserId2, JoinedAt = TimeHelper.UnixTimeNow }
        };

                var newConversation = new Conversation
                {
                    Name = $"Chat between {request.UserId1} and {request.UserId2}",
                    Members = members,
                    Type = "personal",
                    CreatedAt = TimeHelper.UnixTimeNow,
                    LastMessage = string.Empty,
                    LastMessageTime = 0
                };

                await _messageService.SaveConversationAsync(newConversation);

                return Ok(new
                {
                    success = true,
                    message = "Conversation created successfully",
                    data = newConversation
                });
            }
            catch (Exception ex)
            {
                logger.LogError($"Error creating conversation: {ex.Message}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }




        [HttpPost("test")]
        public async Task<IActionResult> SendTestMessage()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Server", "Test message");
            logger.LogInformation($"Client connected");
            return Ok("Test message sent to all clients.");
        }

        // API gọi để gửi tin nhắn
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDTO message, [FromQuery] string userId)
        {
            try
            {
                MessageDTO messageReceive = new()
                {
                    SenderId = message.SenderId,
                    ReceiverId = message.ReceiverId,
                    Content = message.Content,
                    IsDeleted = false,
                    IsPinned = false,
                    IsRead = false,
                    Attachments = null,
                    ConversationId = message.ConversationId
                };
                await _messageService.SaveMessageAsync(messageReceive);

                await _hubContext.Clients.All.SendAsync("ReceiveMessage", userId, message);

                logger.LogInformation($"Message sent from {userId}: {message.Content}");

                return Ok("Message sent successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error sending message: {ex.Message}");
                return StatusCode(500, "Internal Server Error");
            }
        }


        //// Tạo group mới
        //[HttpPost("create-group")]
        //public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupRequest request)
        //{
        //    try
        //    {
        //        // Validate input
        //        if (string.IsNullOrWhiteSpace(request.Name))
        //            return BadRequest("Tên nhóm không được để trống");

        //        if (request.CreatorId == null)
        //            return BadRequest("Phải có người tạo nhóm");

        //        // Tạo conversation mới
        //        var conversation = new Conversation
        //        {
        //            Name = request.Name,
        //            Type = "group",
        //            CreatorId = request.CreatorId,
        //            Members = new List<string> { request.CreatorId },
        //            Admins = new List<string> { request.CreatorId },
        //            Description = request.Description,
        //            GroupAvatar = request.GroupAvatar,
        //            IsPublic = request.IsPublic,
        //            MaxMembers = request.MaxMembers ?? 100,
        //            JoinCode = GenerateUniqueJoinCode()
        //        };

        //        // Lưu vào database
        //        await _messageService.SaveConversationAsync(conversation);

        //        return Ok(new
        //        {
        //            message = "Tạo nhóm thành công",
        //            conversationId = conversation.ConversationId,
        //            joinCode = conversation.JoinCode
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Lỗi tạo nhóm: {ex.Message}");
        //        return StatusCode(500, "Có lỗi xảy ra khi tạo nhóm");
        //    }
        //}

        // Thêm thành viên vào nhóm
        [HttpPost("{conversationId}/add-members")]
        public async Task<IActionResult> AddMembersToGroup(
            string conversationId,
            [FromBody] AddMembersRequest request)
        {
            try
            {
                if (request.MemberIds == null || !request.MemberIds.Any())
                    return BadRequest("Danh sách thành viên trống");

                var conversation = await _messageService.GetConversationByIdAsync(conversationId);
                if (conversation == null)
                    return NotFound("Nhóm không tồn tại");

                // Kiểm tra quyền theo cài đặt
                var canAdd = conversation.PrivacySettings.WhoCanAddMembers == "all_members" ||
                             (conversation.PrivacySettings.WhoCanAddMembers == "admins" &&
                              conversation.Admins.Contains(request.RequestedById));

                if (!canAdd)
                    return Forbid("Bạn không có quyền thêm thành viên");

                // Kiểm tra giới hạn thành viên
                if (conversation.Members.Count + request.MemberIds.Count > conversation.MaxMembers)
                    return BadRequest("Vượt quá số lượng thành viên cho phép");

                var existingIds = conversation.Members.Select(m => m.UserId).ToHashSet();
                var bannedIds = conversation.BannedMembers.ToHashSet();

                var newMemberIds = request.MemberIds
                    .Where(id => !existingIds.Contains(id) && !bannedIds.Contains(id))
                    .ToList();

                foreach (var id in newMemberIds)
                {
                    conversation.Members.Add(new GroupMember
                    {
                        UserId = id,
                        JoinedAt = TimeHelper.UnixTimeNow
                    });
                }

                conversation.UpdatedAt = TimeHelper.UnixTimeNow;
                await _messageService.UpdateConversationAsync(conversation);

                return Ok(new
                {
                    message = $"Đã thêm {newMemberIds.Count} thành viên",
                    addedMembers = newMemberIds
                });
            }
            catch (Exception ex)
            {
                logger.LogError($"Lỗi thêm thành viên: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi thêm thành viên");
            }
        }


        // Xóa thành viên khỏi nhóm
        [HttpDelete("{conversationId}/remove-member/{memberId}")]
        public async Task<IActionResult> RemoveMemberFromGroup(
            string conversationId,
            string memberId,
            [FromQuery] string requestedById)
        {
            try
            {
                var conversation = await _messageService.GetConversationByIdAsync(conversationId);
                if (conversation == null)
                    return NotFound("Nhóm không tồn tại");

                if (!conversation.Admins.Contains(requestedById))
                    return Forbid("Bạn không có quyền xóa thành viên");

                if (conversation.Admins.Contains(memberId))
                    return BadRequest("Không thể xóa quản trị viên");

                var memberToRemove = conversation.Members.FirstOrDefault(m => m.UserId == memberId);
                if (memberToRemove == null)
                    return NotFound("Thành viên không tồn tại trong nhóm");

                conversation.Members.Remove(memberToRemove);
                conversation.UpdatedAt = TimeHelper.UnixTimeNow;

                await _messageService.UpdateConversationAsync(conversation);

                return Ok("Đã xóa thành viên khỏi nhóm");
            }
            catch (Exception ex)
            {
                logger.LogError($"Lỗi xóa thành viên: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi xóa thành viên");
            }
        }


        // Quản lý admin (thêm/xóa)
        [HttpPost("{conversationId}/manage-admin")]
        public async Task<IActionResult> ManageGroupAdmin(
            string conversationId,
            [FromBody] ManageAdminRequest request)
        {
            try
            {
                var conversation = await _messageService.GetConversationByIdAsync(conversationId);
                if (conversation == null)
                    return NotFound("Nhóm không tồn tại");

                if (conversation.CreatorId != request.RequestedById)
                    return Forbid("Chỉ chủ nhóm mới được quản lý admin");

                var isMember = conversation.Members.Any(m => m.UserId == request.UserId);
                if (!isMember)
                    return BadRequest("Người dùng phải là thành viên nhóm");

                if (request.IsAddAdmin)
                {
                    if (!conversation.Admins.Contains(request.UserId))
                        conversation.Admins.Add(request.UserId);
                }
                else
                {
                    conversation.Admins.Remove(request.UserId);
                }

                conversation.UpdatedAt = TimeHelper.UnixTimeNow;
                await _messageService.UpdateConversationAsync(conversation);

                return Ok($"{(request.IsAddAdmin ? "Thêm" : "Xóa")} admin thành công");
            }
            catch (Exception ex)
            {
                logger.LogError($"Lỗi quản lý admin: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi quản lý admin");
            }
        }

        // API cập nhật thông tin nhóm
        [HttpPut("{conversationId}")]
        public async Task<IActionResult> UpdateGroupInfo(
            string conversationId,
            [FromBody] UpdateGroupInfoRequest request)
        {
            try
            {
                var conversation = await _messageService.GetConversationByIdAsync(conversationId);
                if (conversation == null)
                    return NotFound("Nhóm không tồn tại");

                // Chỉ admin mới được sửa
                if (!conversation.Admins.Contains(request.RequestedById))
                    return Forbid("Bạn không có quyền sửa thông tin nhóm");

                // Cập nhật từng trường
                if (!string.IsNullOrWhiteSpace(request.Name))
                    conversation.Name = request.Name;

                if (!string.IsNullOrWhiteSpace(request.Description))
                    conversation.Description = request.Description;

                if (!string.IsNullOrWhiteSpace(request.GroupAvatar))
                    conversation.GroupAvatar = request.GroupAvatar;

                conversation.IsPublic = request.IsPublic ?? conversation.IsPublic;
                conversation.MaxMembers = request.MaxMembers ?? conversation.MaxMembers;
                conversation.UpdatedAt = TimeHelper.UnixTimeNow;

                await _messageService.UpdateConversationAsync(conversation);

                return Ok("Cập nhật thông tin nhóm thành công");
            }
            catch (Exception ex)
            {
                logger.LogError($"Lỗi cập nhật nhóm: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật nhóm");
            }

        }
        // Sinh mã join ngẫu nhiên
        private string GenerateUniqueJoinCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }

    }
}
