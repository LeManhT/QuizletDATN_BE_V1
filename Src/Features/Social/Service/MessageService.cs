using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Quizlet_App_Server.Src.DTO;
using Quizlet_App_Server.Src.Features.Social.Models;
using Quizlet_App_Server.Src.Mapping;
using Quizlet_App_Server.Utility;

namespace Quizlet_App_Server.Src.Features.Social.Service
{
    public class MessageService
    {
        private readonly IMongoCollection<Message> _messageCollection;
        private readonly IMongoCollection<Conversation> _conversationCollection;
        protected readonly IMongoClient client;
        private readonly IConfiguration config;
        private readonly ILogger<MessageService> _logger;

        public MessageService(IMongoClient client, IConfiguration config, ILogger<MessageService> logger)
        {
            var database = client.GetDatabase(VariableConfig.DatabaseName);
            _messageCollection = database.GetCollection<Message>("messages");
            _conversationCollection = database.GetCollection<Conversation>("conversation");
            this.client = client;
            this.config = config;
            this._logger = logger;
        }

        public async Task SaveMessageAsync(MessageDTO messageDto)
        {
            try
            {
                // Sử dụng AutoMapper để chuyển đổi từ MessageDTO sang Message
                var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
                var mapper = config.CreateMapper();

                var message = mapper.Map<Message>(messageDto);
                _logger.LogInformation($"Saving message to MongoDB: {message.Content}");
                await _messageCollection.InsertOneAsync(message);
                _logger.LogInformation("Insert into collection");
                _logger.LogInformation("Message saved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Error saving message: {ex.Message}");
            }
        }

        // Lấy danh sách tin nhắn giữa hai người dùng (theo thứ tự thời gian)
        public async Task<List<Message>> GetChatMessagesAsync(string userId1, string userId2)
        {
            var filter = Builders<Message>.Filter.Or(
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderId, userId1),
                    Builders<Message>.Filter.Eq(m => m.ReceiverId, userId2)
                ),
                Builders<Message>.Filter.And(
                    Builders<Message>.Filter.Eq(m => m.SenderId, userId2),
                    Builders<Message>.Filter.Eq(m => m.ReceiverId, userId1)
                )
            );

            return await _messageCollection.Find(filter).SortBy(m => m.Timestamp).ToListAsync();
        }

        public async Task<List<Message>> GetUnreadMessagesAsync(string userId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.ReceiverId, userId);
            return await _messageCollection.Find(filter).SortBy(m => m.Timestamp).ToListAsync();
        }

        // Xóa tin nhắn theo MessageId
        public async Task DeleteMessageAsync(string messageId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.MessageId, messageId);

            var result = await _messageCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                throw new KeyNotFoundException("Message not found.");
            }
        }

        // Lưu cuộc hội thoại vào database
        public async Task SaveConversationAsync(Conversation conversation)
        {
            await _conversationCollection.InsertOneAsync(conversation);
        }

        public async Task<List<Conversation>> GetUserConversationsAsync(string userId)
        {
            var filter = Builders<Conversation>.Filter.ElemMatch(c => c.Members, m => m.UserId == userId);
            return await _conversationCollection
                .Find(filter)
                .SortByDescending(c => c.LastMessageTime)
                .ToListAsync();
        }



        // Ghim hoặc bỏ ghim tin nhắn
        public async Task<bool> PinMessageAsync(string messageId, bool isPinned)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.MessageId, messageId);
            var update = Builders<Message>.Update.Set(m => m.IsPinned, isPinned);

            var result = await _messageCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        // Lấy danh sách tin nhắn của cuộc trò chuyện
        public async Task<List<Message>> GetMessagesByConversationAsync(string conversationId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId);
            return await _messageCollection.Find(filter).SortByDescending(m => m.Timestamp).ToListAsync();
        }

        // Lấy danh sách tin nhắn đã ghim
        public async Task<List<Message>> GetPinnedMessagesAsync(string conversationId)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId),
                Builders<Message>.Filter.Eq(m => m.IsPinned, true)
            );

            return await _messageCollection.Find(filter).ToListAsync();
        }

        // Lấy danh sách cuộc hội thoại của một người dùng
        public async Task<Conversation> GetConversationAsync(string userId1, string userId2)
        {
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.ElemMatch(c => c.Members, m => m.UserId == userId1),
                Builders<Conversation>.Filter.ElemMatch(c => c.Members, m => m.UserId == userId2),
                Builders<Conversation>.Filter.Eq(c => c.Type, "personal")
            );

            return await _conversationCollection.Find(filter).FirstOrDefaultAsync();
        }

        // Lấy thông tin conversation theo ID
        public async Task<Conversation?> GetConversationByIdAsync(string conversationId)
        {
            return await _conversationCollection
                .Find(c => c.Id == conversationId)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateConversationAsync(Conversation conversation)
        {
            try
            {
                // Kiểm tra conversation có null không
                if (conversation == null)
                    throw new ArgumentNullException(nameof(conversation), "Conversation không được null");

                // Kiểm tra conversation có ID không
                if (string.IsNullOrWhiteSpace(conversation.Id))
                    throw new InvalidOperationException("Conversation phải có ID");

                // Tìm conversation theo ID
                var filter = Builders<Conversation>.Filter.Eq(c => c.Id, conversation.Id);

                // Tạo update builder
                var updateDefinition = Builders<Conversation>.Update
                    .Set(c => c.Name, conversation.Name)
                    .Set(c => c.Type, conversation.Type)
                    .Set(c => c.Members, conversation.Members)
                    .Set(c => c.Admins, conversation.Admins)
                    .Set(c => c.CreatorId, conversation.CreatorId)
                    .Set(c => c.Description, conversation.Description)
                    .Set(c => c.GroupAvatar, conversation.GroupAvatar)
                    .Set(c => c.IsPublic, conversation.IsPublic)
                    .Set(c => c.MaxMembers, conversation.MaxMembers)
                    .Set(c => c.JoinCode, conversation.JoinCode)
                    .Set(c => c.LastMessage, conversation.LastMessage)
                    .Set(c => c.LastMessageTime, conversation.LastMessageTime)
                    .Set(c => c.UpdatedAt, TimeHelper.UnixTimeNow)
                    .Set(c => c.BannedMembers, conversation.BannedMembers ?? new List<string>());

                // Thực hiện update
                var result = await _conversationCollection.UpdateOneAsync(filter, updateDefinition);

                // Kiểm tra kết quả update
                if (result.ModifiedCount == 0)
                {
                    _logger.LogWarning($"Không tìm thấy conversation để cập nhật: {conversation.Id}");
                    throw new InvalidOperationException($"Không tìm thấy conversation với ID: {conversation.Id}");
                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(ex, $"Lỗi khi cập nhật conversation: {ex.Message}");
                throw; // Re-throw để cho phép xử lý ở tầng gọi
            }
        }
    }

}
