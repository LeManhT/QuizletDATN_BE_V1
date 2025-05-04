
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Controller;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Features.Social.Service;
using Quizlet_App_Server.Src.Mapping;
using System.Threading.Tasks;
namespace Quizlet_App_Server.Src
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;
        private readonly ILogger<ChatHub> logger;
        private readonly GroupService _groupService;
        private readonly IMapper _mapper;

        public ChatHub(MessageService messageService, ILogger<ChatHub> logger, GroupService groupService)
        {
            _messageService = messageService;
            _groupService = groupService;
            this.logger = logger;
            logger.LogInformation("ChatHub initialized with MessageService.");
            // Khởi tạo AutoMapper thủ công
            var configMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configMapper.CreateMapper();
        }
        public async Task SendMessage(string userId, MessageDTO message)
        {
            try
            {
                logger.LogInformation("Informationnnnnnn : ", $"Received message from {userId}: {message.Content}");
                await _messageService.SaveMessageAsync(message);
                await Clients.All.SendAsync("ReceiveMessage", userId, message);
                logger.LogInformation("Informationnnnnnn : ", "Message saved and broadcasted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendMessage: {ex.Message}");
            }
        }

        public async Task JoinGroup(string userId, string groupId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
                logger.LogInformation("User {UserId} with connection {ConnectionId} joined group {GroupId}",
                    userId, Context.ConnectionId, groupId);

                // Notify group members
                await Clients.Group(groupId).SendAsync("UserJoinedGroup", userId, groupId);

                // Get group details to send to the joining user
                var group = await _groupService.GetGroupByIdAsync(groupId);
                await Clients.Caller.SendAsync("JoinedGroupSuccess", _mapper.Map<ConversationDTO>(group));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in JoinGroup");
                await Clients.Caller.SendAsync("JoinedGroupError", ex.Message);
            }
        }

        public async Task LeaveGroup(string userId, string groupId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
                logger.LogInformation("User {UserId} with connection {ConnectionId} left group {GroupId}",
                    userId, Context.ConnectionId, groupId);

                // Notify group members
                await Clients.Group(groupId).SendAsync("UserLeftGroup", userId, groupId);
                await Clients.Caller.SendAsync("LeftGroupSuccess", groupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in LeaveGroup");
                await Clients.Caller.SendAsync("LeftGroupError", ex.Message);
            }
        }

        public async Task SendGroupMessage(string userId, string groupId, GroupMessageDTO message)
        {
            try
            {
                logger.LogInformation("Group message from {UserId} to {GroupId}: {Content}",
                    userId, groupId, message.Content);

                // Convert to Message and save
                var messageEntity = _mapper.Map<Message>(message);
                await _messageService.SaveMessageAsync(_mapper.Map<MessageDTO>(messageEntity));

                // Send to all members in the group
                await Clients.Group(groupId).SendAsync("ReceiveGroupMessage", userId, groupId, message);

                logger.LogInformation("Group message saved and broadcasted to group {GroupId}", groupId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in SendGroupMessage");
                await Clients.Caller.SendAsync("SendGroupMessageError", ex.Message);
            }
        }

        public async Task GetGroupMessages(string groupId, int limit = 50, long? beforeTimestamp = null)
        {
            try
            {
                var messages = await _groupService.GetGroupMessagesAsync(groupId, limit, beforeTimestamp);
                await Clients.Caller.SendAsync("ReceiveGroupMessages", groupId, messages);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetGroupMessages");
                await Clients.Caller.SendAsync("GetGroupMessagesError", ex.Message);
            }
        }

        public async Task CreateGroup(string userId, ConversationDTO groupDto)
        {
            try
            {
                // Map and create the group
                var group = _mapper.Map<Conversation>(groupDto);
                var createdGroup = await _groupService.CreateGroupAsync(group);

                // Auto-join the creator to the SignalR group
                await Groups.AddToGroupAsync(Context.ConnectionId, createdGroup.Id);

                // Notify the creator
                await Clients.Caller.SendAsync("GroupCreated", _mapper.Map<ConversationDTO>(createdGroup));

                logger.LogInformation("Group created successfully: {GroupId} by user {UserId}",
                    createdGroup.Id, userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CreateGroup");
                await Clients.Caller.SendAsync("CreateGroupError", ex.Message);
            }
        }

        public async Task AddMemberToGroup(string groupId, GroupMemberDTO memberDto)
        {
            try
            {
                var member = _mapper.Map<GroupMember>(memberDto);
                var result = await _groupService.AddMemberToGroupAsync(groupId, member);

                if (result)
                {
                    // Notify all members in the group
                    await Clients.Group(groupId).SendAsync("MemberAddedToGroup", groupId, memberDto);
                    logger.LogInformation("Member {UserId} added to group {GroupId}",
                        memberDto.UserId, groupId);
                }
                else
                {
                    await Clients.Caller.SendAsync("AddMemberToGroupError", "Failed to add member to group");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddMemberToGroup");
                await Clients.Caller.SendAsync("AddMemberToGroupError", ex.Message);
            }
        }


        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            logger.LogInformation($"Client connected: {Context.ConnectionId}");
            //Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            logger.LogInformation("Info=","Client disconnected: {Context.ConnectionId}");
        }

        public async Task TestConnection()
        {
            await Clients.All.SendAsync("ReceiveMessage", "Server", "Test message");
        }


    }

}
